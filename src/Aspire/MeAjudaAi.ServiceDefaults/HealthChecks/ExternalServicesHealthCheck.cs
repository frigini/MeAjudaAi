using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeAjudaAi.ServiceDefaults.HealthChecks;

public class ExternalServicesHealthCheck(HttpClient httpClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ Serviços externos reais:
            // - Keycloak (se não for local)
            // - APIs de pagamento (PagSeguro, Stripe)
            // - APIs de geolocalização (Google Maps)
            // - Serviços de email (SendGrid)
            // - APIs de SMS

            // Exemplo: Check do Keycloak
            var response = await httpClient.GetAsync("http://localhost:8080/health", cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("External services accessible")
                : HealthCheckResult.Degraded("Some external services unavailable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("External services check failed", ex);
        }
    }
}