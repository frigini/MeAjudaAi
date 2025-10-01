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

            if (healthyCount == 0)
            {
                var errors = string.Join("; ", results.Where(r => !r.IsHealthy).Select(r => $"{r.Service}: {r.Error}"));
                return HealthCheckResult.Unhealthy($"All external services are down: {errors}");
            }

            var partialErrors = string.Join("; ", results.Where(r => !r.IsHealthy).Select(r => $"{r.Service}: {r.Error}"));
            return HealthCheckResult.Degraded($"{healthyCount}/{totalCount} services healthy. Issues: {partialErrors}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during external services health check");
            return HealthCheckResult.Unhealthy("Health check falhou com erro inesperado", ex);
        }
    }

    private async Task<(bool IsHealthy, string? Error)> CheckKeycloakAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(externalServicesOptions.Keycloak.BaseUrl))
                return (false, "BaseUrl not configured");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(externalServicesOptions.Keycloak.TimeoutSeconds));

            var baseUri = externalServicesOptions.Keycloak.BaseUrl.TrimEnd('/');
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUri}/health");
            var response = await httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
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

    private async Task<(bool IsHealthy, string? Error)> CheckPaymentGatewayAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(externalServicesOptions.PaymentGateway.BaseUrl))
                return (false, "BaseUrl not configured");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(externalServicesOptions.PaymentGateway.TimeoutSeconds));

            var baseUri = externalServicesOptions.PaymentGateway.BaseUrl.TrimEnd('/');
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUri}/health");
            var response = await httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
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

    private async Task<(bool IsHealthy, string? Error)> CheckGeolocationAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(externalServicesOptions.Geolocation.BaseUrl))
                return (false, "BaseUrl not configured");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(externalServicesOptions.Geolocation.TimeoutSeconds));

            var baseUri = externalServicesOptions.Geolocation.BaseUrl.TrimEnd('/');
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUri}/health");
            var response = await httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
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
    public int TimeoutSeconds { get; set; } = 5;
}

/// <summary>
/// Opções de configuração para health check do gateway de pagamento
/// </summary>
public class PaymentGatewayHealthOptions
{
    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
}

/// <summary>
/// Opções de configuração para health check do serviço de geolocalização
/// </summary>
public class GeolocationHealthOptions
{
    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}