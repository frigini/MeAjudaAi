using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MeAjudaAi.ServiceDefaults.Options;

namespace MeAjudaAi.ServiceDefaults.HealthChecks;

/// <summary>
/// Health check para verificar a conectividade com serviços externos
/// </summary>
public class ExternalServicesHealthCheck(
    HttpClient httpClient,
    ExternalServicesOptions externalServicesOptions,
    ILogger<ExternalServicesHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var results = new List<(string Service, bool IsHealthy, string? Error)>();

        try
        {
            // Verifica o Keycloak se estiver habilitado
            if (externalServicesOptions.Keycloak.Enabled)
            {
                var (IsHealthy, Error) = await CheckKeycloakAsync(cancellationToken);
                results.Add(("Keycloak", IsHealthy, Error));
            }

            // Verifica serviços de geolocalização (implementação futura)
            if (externalServicesOptions.Geolocation.Enabled)
            {
                var (IsHealthy, Error) = await CheckGeolocationAsync(cancellationToken);
                results.Add(("Geolocation Service", IsHealthy, Error));
            }

            var healthyCount = results.Count(r => r.IsHealthy);
            var totalCount = results.Count;

            if (totalCount == 0)
            {
                return HealthCheckResult.Healthy("Nenhum serviço externo configurado");
            }

            if (healthyCount == totalCount)
            {
                return HealthCheckResult.Healthy($"Todos os {totalCount} serviços externos estão saudáveis");
            }

            // Serviços externos inativos nunca devem tornar a aplicação unhealthy (apenas degraded)
            // A aplicação pode continuar a funcionar com recursos limitados quando serviços externos estão indisponíveis
            var issues = results.Where(r => !r.IsHealthy).ToArray();
            var message = $"{healthyCount}/{totalCount} serviços saudáveis";
            
            // Estrutura erros por nome do serviço para facilitar monitoramento/alertas
            // Usa construção manual de dicionário para lidar com possíveis nomes de serviço duplicados
            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var issue in issues)
            {
                data[issue.Service] = issue.Error ?? "Erro desconhecido";
            }
            
            return HealthCheckResult.Degraded(message, data: data);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Deixa a camada de hospedagem lidar com semânticas de cancelamento ao invés de tratá-lo como falha
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado durante verificação de saúde dos serviços externos");
            return HealthCheckResult.Unhealthy("Verificação de saúde falhou com erro inesperado", ex);
        }
    }

    /// <summary>
    /// Lógica comum de health check para serviços externos
    /// </summary>
    private async Task<(bool IsHealthy, string? Error)> CheckServiceAsync(
        string baseUrl, int timeoutSeconds, string healthEndpointPath, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return (false, "BaseUrl não configurada");

            if (timeoutSeconds <= 0)
                return (false, "Configuração de timeout inválida");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            var baseUri = baseUrl.TrimEnd('/');
            // Normaliza o caminho: padrão para "/health", remove espaços, garante '/' inicial único
            var normalizedPath = string.IsNullOrWhiteSpace(healthEndpointPath) 
                ? "/health" 
                : "/" + healthEndpointPath.Trim().TrimStart('/');
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUri}{normalizedPath}");
            using var response = await httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                .ConfigureAwait(false);

            return response.IsSuccessStatusCode
                ? (true, null)
                : (false, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (false, "Timeout da requisição");
        }
        catch (UriFormatException)
        {
            return (false, "URL inválida");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Falha na conexão: {ex.Message}");
        }
    }

    private Task<(bool IsHealthy, string? Error)> CheckKeycloakAsync(CancellationToken cancellationToken) =>
        CheckServiceAsync(
            externalServicesOptions.Keycloak.BaseUrl,
            externalServicesOptions.Keycloak.TimeoutSeconds,
            externalServicesOptions.Keycloak.HealthEndpointPath,
            cancellationToken);

    private Task<(bool IsHealthy, string? Error)> CheckGeolocationAsync(CancellationToken cancellationToken) =>
        CheckServiceAsync(
            externalServicesOptions.Geolocation.BaseUrl,
            externalServicesOptions.Geolocation.TimeoutSeconds,
            externalServicesOptions.Geolocation.HealthEndpointPath,
            cancellationToken);
}
