using Polly.CircuitBreaker;
using System.Net;

namespace MeAjudaAi.Web.Admin.Services.Resilience;

/// <summary>
/// Handler para logar e rastrear exceções do Polly
/// </summary>
public class PollyLoggingHandler : DelegatingHandler
{
    private readonly ILogger<PollyLoggingHandler> _logger;
    private readonly IConnectionStatusService _connectionStatus;

    public PollyLoggingHandler(
        ILogger<PollyLoggingHandler> logger,
        IConnectionStatusService connectionStatus)
    {
        _logger = logger;
        _connectionStatus = connectionStatus;
    }

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
                _connectionStatus.CurrentStatus != ConnectionStatus.Connected)
            {
                _connectionStatus.UpdateStatus(ConnectionStatus.Connected);
            }

            return response;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, 
                "🔴 Circuit breaker is open - API unavailable. Request: {RequestUri}",
                request.RequestUri);
            
            _connectionStatus.UpdateStatus(ConnectionStatus.Disconnected);
            
            // Retorna resposta 503 Service Unavailable
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                RequestMessage = request,
                Content = new StringContent(
                    "O serviço está temporariamente indisponível. " +
                    "Aguarde alguns instantes e tente novamente.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "❌ Unexpected error during HTTP request: {RequestUri}",
                request.RequestUri);
            
            _connectionStatus.UpdateStatus(ConnectionStatus.Reconnecting);
            throw;
        }
    }
}
