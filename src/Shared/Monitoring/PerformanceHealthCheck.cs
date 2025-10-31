using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeAjudaAi.Shared.Monitoring;

public partial class MeAjudaAiHealthChecks
{
    /// <summary>
    /// Health check para verificar métricas de performance
    /// </summary>
    internal class PerformanceHealthCheck() : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Verificar métricas de performance
                var memoryUsage = GC.GetTotalMemory(false);
                var memoryUsageMB = memoryUsage / 1024 / 1024;

                var data = new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow },
                    { "memory_usage_mb", memoryUsageMB },
                    { "gc_gen0_collections", GC.CollectionCount(0) },
                    { "gc_gen1_collections", GC.CollectionCount(1) },
                    { "gc_gen2_collections", GC.CollectionCount(2) },
                    { "thread_pool_worker_threads", ThreadPool.ThreadCount }
                };

                // Alertar se o uso de memória estiver muito alto
                if (memoryUsageMB > 500) // 500MB threshold
                {
                    return Task.FromResult(
                        HealthCheckResult.Degraded("High memory usage detected", data: data));
                }

                return Task.FromResult(
                    HealthCheckResult.Healthy("Performance metrics are within normal ranges", data));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy("Failed to collect performance metrics", ex));
            }
        }
    }
}
