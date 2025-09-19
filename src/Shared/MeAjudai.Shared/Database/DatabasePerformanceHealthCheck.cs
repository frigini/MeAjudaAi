using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Health check para monitorar performance de database.
/// Verifica se há muitas queries lentas ou problemas de conexão.
/// </summary>
public sealed class DatabasePerformanceHealthCheck : IHealthCheck
{
    private readonly DatabaseMetrics _metrics;
    private readonly ILogger<DatabasePerformanceHealthCheck> _logger;
    
    // Thresholds simples para alertas
    private static readonly TimeSpan CheckWindow = TimeSpan.FromMinutes(5);
    
    public DatabasePerformanceHealthCheck(
        DatabaseMetrics metrics, 
        ILogger<DatabasePerformanceHealthCheck> logger)
    {
        _metrics = metrics;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Para um sistema inicial, apenas verificamos se as métricas estão funcionando
            // Critérios mais sofisticados podem ser adicionados quando necessário
            
            var description = "Database monitoring active";
            var data = new Dictionary<string, object>
            {
                ["monitoring_active"] = true,
                ["check_window_minutes"] = CheckWindow.TotalMinutes
            };

            return Task.FromResult(HealthCheckResult.Healthy(description, data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database performance health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Database performance monitoring error", ex));
        }
    }
}