using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.ServiceDefaults.HealthChecks;

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
            // Check Keycloak if enabled
            if (externalServicesOptions.Keycloak.Enabled)
            {
                var (IsHealthy, Error)= await CheckKeycloakAsync(cancellationToken);
                results.Add(("Keycloak", IsHealthy, Error));
            }

            // Check external payment APIs (future implementation)
            if (externalServicesOptions.PaymentGateway.Enabled)
            {
                var (IsHealthy, Error)= await CheckPaymentGatewayAsync(cancellationToken);
                results.Add(("Payment Gateway", IsHealthy, Error));
            }

            // Check geolocation services (future implementation)
            if (externalServicesOptions.Geolocation.Enabled)
            {
                var (IsHealthy, Error)= await CheckGeolocationAsync(cancellationToken);
                results.Add(("Geolocation Service", IsHealthy, Error));
            }

            var healthyCount = results.Count(r => r.IsHealthy);
            var totalCount = results.Count;

            if (totalCount == 0)
            {
                return HealthCheckResult.Healthy("No external services configured");
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
            return HealthCheckResult.Unhealthy("Health check failed with unexpected error", ex);
        }
    }

    private async Task<(bool IsHealthy, string? Error)> CheckKeycloakAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"{externalServicesOptions.Keycloak.BaseUrl}/health", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            
            return (false, $"HTTP {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Connection failed: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, "Request timeout");
        }
    }

    private static async Task<(bool IsHealthy, string? Error)> CheckPaymentGatewayAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Placeholder for payment gateway health check
            // Implementation depends on the specific payment provider (PagSeguro, Stripe, etc.)
            await Task.Delay(10, cancellationToken); // Simulate API call
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static async Task<(bool IsHealthy, string? Error)> CheckGeolocationAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Placeholder for geolocation service health check (Google Maps, HERE, etc.)
            await Task.Delay(10, cancellationToken); // Simulate API call
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}

/// <summary>
/// Configuration options for external services health checks
/// </summary>
public class ExternalServicesOptions
{
    public const string SectionName = "ExternalServices";

    public KeycloakHealthOptions Keycloak { get; set; } = new();
    public PaymentGatewayHealthOptions PaymentGateway { get; set; } = new();
    public GeolocationHealthOptions Geolocation { get; set; } = new();
}

public class KeycloakHealthOptions
{
    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = "http://localhost:8080";
    public int TimeoutSeconds { get; set; } = 5;
}

public class PaymentGatewayHealthOptions
{
    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
}

public class GeolocationHealthOptions
{
    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}