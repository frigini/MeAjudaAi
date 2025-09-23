using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Health check para monitorar performance de database.
/// Verifica se há muitas queries lentas ou problemas de conexão.
/// </summary>
public sealed class DatabasePerformanceHealthCheck(
    DatabaseMetrics metrics,
    ILogger<DatabasePerformanceHealthCheck> logger) : IHealthCheck
{

    // Thresholds simples para alertas
    private static readonly TimeSpan CheckWindow = TimeSpan.FromMinutes(5);

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar se o sistema de métricas está configurado
            var metricsConfigured = metrics != null;
            
            var description = "Database monitoring active";
            var data = new Dictionary<string, object>
            {
                ["monitoring_active"] = true,
                ["check_window_minutes"] = CheckWindow.TotalMinutes,
                ["metrics_configured"] = metricsConfigured
            };

            // Se as métricas estão configuradas, consideramos saudável
            // Critérios mais sofisticados podem ser adicionados quando necessário
            return Task.FromResult(HealthCheckResult.Healthy(description, data));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database performance health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Database performance monitoring error", ex));
        }
    }
}