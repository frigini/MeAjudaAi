using MeAjudaAi.Web.Admin.Services.Resilience.Interfaces;
using Polly.CircuitBreaker;
using System.Net;

namespace MeAjudaAi.Web.Admin.Services.Resilience.Http;

/// <summary>
/// Handler para logar e rastrear exceções do Polly
/// </summary>
public class PollyLoggingHandler(
    ILogger<PollyLoggingHandler> logger,
    IConnectionStatusService connectionStatus) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Adiciona URI ao contexto do Polly para logging usando a API recomendada
            if (request.Options.TryGetValue(new HttpRequestOptionsKey<Polly.Context>("PolicyExecutionContext"), out var context))
            {
                context["requestUri"] = request.RequestUri?.ToString() ?? "Unknown";
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Restaura status se sucesso após falhas
            if (response.IsSuccessStatusCode && 
                connectionStatus.CurrentStatus != ConnectionStatus.Connected)
            {
                connectionStatus.UpdateStatus(ConnectionStatus.Connected);
            }

            return response;
        }
        catch (BrokenCircuitException ex)
        {
            logger.LogError(ex, 
                "🔴 Circuit breaker is open - API unavailable. Request: {RequestUri}. Total retries/failures reached threshold.",
                request.RequestUri);
            
            connectionStatus.UpdateStatus(ConnectionStatus.Disconnected);
            
            // Retorna resposta 503 Service Unavailable
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                RequestMessage = request,
                Content = new StringContent(
                    "O serviço está temporariamente indisponível devido a falhas técnicas. " +
                    "Aguarde alguns instantes enquanto tentamos restabelecer a conexão.")
            };
        }
        #pragma warning disable S2139 // Log and rethrow to maintain observability
        catch (Exception ex)
        #pragma warning restore S2139
        {
            logger.LogError(ex, 
                "❌ Unexpected error during HTTP request: {RequestUri}",
                request.RequestUri);
            
            
            connectionStatus.UpdateStatus(ConnectionStatus.Reconnecting);
            #pragma warning disable S2139 // Log and rethrow to maintain observability
            throw;
            #pragma warning restore S2139
        }
    }
}
