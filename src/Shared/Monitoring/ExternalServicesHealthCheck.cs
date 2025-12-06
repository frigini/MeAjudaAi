using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeAjudaAi.Shared.Monitoring;

public partial class MeAjudaAiHealthChecks
{
    /// <summary>
    /// Health check para verificar disponibilidade de serviços externos
    /// </summary>
    /// <param name="httpClient">Cliente HTTP para realizar requisições aos serviços externos</param>
    /// <param name="configuration">Configuração da aplicação contendo endpoints e configurações dos serviços</param>
    public class ExternalServicesHealthCheck(HttpClient httpClient, IConfiguration configuration) : IHealthCheck
    {
        /// <summary>
        /// Verifica a disponibilidade dos serviços externos configurados
        /// </summary>
        /// <param name="context">Contexto da verificação de saúde</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resultado da verificação de saúde</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, object>();
            var allHealthy = true;

            // Verificar Keycloak
            try
            {
                var keycloakUrl = configuration["Keycloak:BaseUrl"];
                if (!string.IsNullOrEmpty(keycloakUrl))
                {
                    using var response = await httpClient.GetAsync($"{keycloakUrl}/realms/meajudaai", cancellationToken);
                    results["keycloak"] = new
                    {
                        status = response.IsSuccessStatusCode ? "healthy" : "unhealthy",
                        response_time_ms = 0 // Could measure actual response time
                    };

                    if (!response.IsSuccessStatusCode)
                        allHealthy = false;
                }
            }
            catch (Exception ex)
            {
                results["keycloak"] = new { status = "unhealthy", error = ex.Message };
                allHealthy = false;
            }

            // Verificar outros serviços externos aqui...

            results["timestamp"] = DateTime.UtcNow;
            results["overall_status"] = allHealthy ? "healthy" : "degraded";

            return allHealthy
                ? HealthCheckResult.Healthy("All external services are operational", results)
                : HealthCheckResult.Degraded("Some external services are not operational", data: results);
        }
    }
}
