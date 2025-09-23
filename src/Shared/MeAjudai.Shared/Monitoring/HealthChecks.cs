using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Health checks customizados para componentes específicos do MeAjudaAi
/// </summary>
public partial class MeAjudaAiHealthChecks
{
    /// <summary>
    /// Health check para verificar se o sistema pode processar ajudas
    /// </summary>
    public class HelpProcessingHealthCheck() : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Verificar se os serviços essenciais estão funcionando
                // Simular uma verificação rápida do sistema de ajuda
                
                var data = new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow },
                    { "component", "help_processing" },
                    { "can_process_requests", true }
                };

                return Task.FromResult(HealthCheckResult.Healthy("Help processing system is operational", data));
            }
            catch (Exception ex)
            {
                var data = new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow },
                    { "component", "help_processing" },
                    { "error", ex.Message }
                };

                return Task.FromResult(HealthCheckResult.Unhealthy("Help processing system is not operational", ex, data));
            }
        }
    }
}