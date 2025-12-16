using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

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

            // Verifica APIs de pagamento externas (implementação futura)
            if (externalServicesOptions.PaymentGateway.Enabled)
            {
                var (IsHealthy, Error) = await CheckPaymentGatewayAsync(cancellationToken);
                results.Add(("Payment Gateway", IsHealthy, Error));
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
                return HealthCheckResult.Healthy("No external service configured");
            }

            if (healthyCount == totalCount)
            {
                return HealthCheckResult.Healthy($"All {totalCount} external services are healthy");
            }

            // External services down should never make the app unhealthy (only degraded)
            // Application can continue to function with limited features when external services are unavailable
            var issues = results.Where(r => !r.IsHealthy).ToArray();
            var message = $"{healthyCount}/{totalCount} services healthy";
            
            // Structure errors by service name for easier monitoring/alerting
            // Use manual dictionary construction to handle potential duplicate service names gracefully
            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var issue in issues)
            {
                data[issue.Service] = issue.Error ?? "Unknown error";
            }
            
            return HealthCheckResult.Degraded(message, data: data);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Let the hosting layer handle cancellation semantics instead of treating it as a failure
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during external services health check");
            return HealthCheckResult.Unhealthy("Health check failed with unexpected error", ex);
        }
    }

    /// <summary>
    /// Common health check logic for external services
    /// </summary>
    private async Task<(bool IsHealthy, string? Error)> CheckServiceAsync(
        string baseUrl, int timeoutSeconds, string healthEndpointPath, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return (false, "BaseUrl not configured");

            if (timeoutSeconds <= 0)
                return (false, "Invalid timeout configuration");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            var baseUri = baseUrl.TrimEnd('/');
            var path = string.IsNullOrWhiteSpace(healthEndpointPath) ? "/health" : healthEndpointPath;
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUri}{path}");
            using var response = await httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                .ConfigureAwait(false);

            return response.IsSuccessStatusCode
                ? (true, null)
                : (false, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (false, "Request timeout");
        }
        catch (UriFormatException)
        {
            return (false, "Invalid URL");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Connection failed: {ex.Message}");
        }
    }

    private Task<(bool IsHealthy, string? Error)> CheckKeycloakAsync(CancellationToken cancellationToken) =>
        CheckServiceAsync(
            externalServicesOptions.Keycloak.BaseUrl,
            externalServicesOptions.Keycloak.TimeoutSeconds,
            externalServicesOptions.Keycloak.HealthEndpointPath,
            cancellationToken);

    private Task<(bool IsHealthy, string? Error)> CheckPaymentGatewayAsync(CancellationToken cancellationToken) =>
        CheckServiceAsync(
            externalServicesOptions.PaymentGateway.BaseUrl,
            externalServicesOptions.PaymentGateway.TimeoutSeconds,
            externalServicesOptions.PaymentGateway.HealthEndpointPath,
            cancellationToken);

    private Task<(bool IsHealthy, string? Error)> CheckGeolocationAsync(CancellationToken cancellationToken) =>
        CheckServiceAsync(
            externalServicesOptions.Geolocation.BaseUrl,
            externalServicesOptions.Geolocation.TimeoutSeconds,
            externalServicesOptions.Geolocation.HealthEndpointPath,
            cancellationToken);
}

/// <summary>
/// Opções de configuração para health checks de serviços externos
/// </summary>
public class ExternalServicesOptions
{
    public const string SectionName = "ExternalServices";

    public KeycloakHealthOptions Keycloak { get; set; } = new();
    public PaymentGatewayHealthOptions PaymentGateway { get; set; } = new();
    public GeolocationHealthOptions Geolocation { get; set; } = new();
}

/// <summary>
/// Opções de configuração para health check do Keycloak
/// </summary>
public class KeycloakHealthOptions
{
    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = "http://localhost:8080";
    public string HealthEndpointPath { get; set; } = "/health";
    public int TimeoutSeconds { get; set; } = 5;
}

/// <summary>
/// Opções de configuração para health check do gateway de pagamento
/// </summary>
public class PaymentGatewayHealthOptions
{
    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = string.Empty;
    public string HealthEndpointPath { get; set; } = "/health";
    public int TimeoutSeconds { get; set; } = 10;
}

/// <summary>
/// Opções de configuração para health check do serviço de geolocalização
/// </summary>
public class GeolocationHealthOptions
{
    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = string.Empty;
    public string HealthEndpointPath { get; set; } = "/health";
    public int TimeoutSeconds { get; set; } = 5;
}
