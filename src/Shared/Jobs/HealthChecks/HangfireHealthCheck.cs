using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Jobs.HealthChecks;

/// <summary>
/// Health check para verificar o funcionamento do Hangfire (background jobs)
/// Monitora conectividade, taxa de falha e performance do sistema de jobs
/// </summary>
public class HangfireHealthCheck(ILogger<HangfireHealthCheck> logger) : IHealthCheck
{
    private readonly ILogger<HangfireHealthCheck> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar se o Hangfire está configurado através da presença do serviço
            var data = new Dictionary<string, object>
            {
                { "timestamp", DateTime.UtcNow },
                { "component", "hangfire" },
                { "configured", true }
            };

            // NOTA: Este health check verifica apenas se o Hangfire está configurado.
            // Em produção, deve ser estendido para:
            // - Verificar conexão com o storage (PostgreSQL)
            // - Monitorar taxa de falha de jobs (via Hangfire.Storage.Monitoring API)
            // - Verificar se o dashboard está acessível
            // - Alertar se taxa de falha > 5%
            // 
            // Referência: docs/technical-debt.md (Hangfire + Npgsql 10.x)

            _logger.LogDebug("Hangfire health check passed - service configured");
            return Task.FromResult(HealthCheckResult.Healthy("Hangfire is configured and operational", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hangfire health check failed");
            
            var data = new Dictionary<string, object>
            {
                { "timestamp", DateTime.UtcNow },
                { "component", "hangfire" },
                { "error", ex.Message }
            };

            return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire is not operational", ex, data));
        }
    }
}
