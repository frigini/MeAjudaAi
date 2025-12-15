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
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    using var response = await httpClient.GetAsync($"{keycloakUrl}/realms/meajudaai", cancellationToken);
                    stopwatch.Stop();

                    results["keycloak"] = new
                    {
                        status = response.IsSuccessStatusCode ? "healthy" : "unhealthy",
                        response_time_ms = stopwatch.ElapsedMilliseconds
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

            // Verificar IBGE API
            try
            {
                var ibgeBaseUrl = configuration["ExternalServices:IbgeApi:BaseUrl"] 
                    ?? "https://servicodados.ibge.gov.br/api/v1/localidades";
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using var response = await httpClient.GetAsync($"{ibgeBaseUrl}/estados", cancellationToken);
                stopwatch.Stop();

                results["ibge_api"] = new
                {
                    status = response.IsSuccessStatusCode ? "healthy" : "unhealthy",
                    response_time_ms = stopwatch.ElapsedMilliseconds,
                    endpoint = "estados"
                };

                if (!response.IsSuccessStatusCode)
                    allHealthy = false;
            }
            catch (Exception ex)
            {
                results["ibge_api"] = new { status = "unhealthy", error = ex.Message };
                allHealthy = false;
            }

            results["timestamp"] = DateTime.UtcNow;
            results["overall_status"] = allHealthy ? "healthy" : "degraded";

            return allHealthy
                ? HealthCheckResult.Healthy("All external services are operational", results)
                : HealthCheckResult.Degraded("Some external services are not operational", data: results);
        }
    }
}
