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
                var (IsHealthy, Error)= await CheckKeycloakAsync(cancellationToken);
                results.Add(("Keycloak", IsHealthy, Error));
            }

            // Verifica APIs de pagamento externas (implementação futura)
            if (externalServicesOptions.PaymentGateway.Enabled)
            {
                var (IsHealthy, Error)= await CheckPaymentGatewayAsync(cancellationToken);
                results.Add(("Gateway de Pagamento", IsHealthy, Error));
            }

            // Verifica serviços de geolocalização (implementação futura)
            if (externalServicesOptions.Geolocation.Enabled)
            {
                var (IsHealthy, Error)= await CheckGeolocationAsync(cancellationToken);
                results.Add(("Serviço de Geolocalização", IsHealthy, Error));
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

            if (healthyCount == 0)
            {
                var errors = string.Join("; ", results.Where(r => !r.IsHealthy).Select(r => $"{r.Service}: {r.Error}"));
                return HealthCheckResult.Unhealthy($"Todos os serviços externos estão fora: {errors}");
            }

            var partialErrors = string.Join("; ", results.Where(r => !r.IsHealthy).Select(r => $"{r.Service}: {r.Error}"));
            return HealthCheckResult.Degraded($"{healthyCount}/{totalCount} serviços saudáveis. Problemas: {partialErrors}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado durante o health check de serviços externos");
            return HealthCheckResult.Unhealthy("Health check falhou com erro inesperado", ex);
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
            return (false, $"Falha na conexão: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, "Tempo limite da requisição");
        }
    }

    private static async Task<(bool IsHealthy, string? Error)> CheckPaymentGatewayAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Placeholder para health check do gateway de pagamento
            // Implementação depende do provedor específico (PagSeguro, Stripe, etc.)
            await Task.Delay(10, cancellationToken); // Simula chamada à API
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
            // Placeholder para health check do serviço de geolocalização (Google Maps, HERE, etc.)
            await Task.Delay(10, cancellationToken); // Simula chamada à API
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
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