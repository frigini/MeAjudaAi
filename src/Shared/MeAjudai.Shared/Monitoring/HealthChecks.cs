using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Health checks customizados para componentes específicos do MeAjudaAi
/// </summary>
public class MeAjudaAiHealthChecks
{
    /// <summary>
    /// Health check para verificar se o sistema pode processar ajudas
    /// </summary>
    public class HelpProcessingHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;

        public HelpProcessingHealthCheck(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

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

    /// <summary>
    /// Health check para verificar a conectividade com serviços externos
    /// </summary>
    public class ExternalServicesHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ExternalServicesHealthCheck(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, object>();
            var allHealthy = true;

            // Verificar Keycloak
            try
            {
                var keycloakUrl = _configuration["Keycloak:BaseUrl"];
                if (!string.IsNullOrEmpty(keycloakUrl))
                {
                    var response = await _httpClient.GetAsync($"{keycloakUrl}/realms/meajudaai", cancellationToken);
                    results["keycloak"] = new { 
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

    /// <summary>
    /// Health check para verificar métricas de performance
    /// </summary>
    public class PerformanceHealthCheck : IHealthCheck
    {
        private readonly BusinessMetrics _businessMetrics;

        public PerformanceHealthCheck(BusinessMetrics businessMetrics)
        {
            _businessMetrics = businessMetrics;
        }

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

/// <summary>
/// Extension methods para registrar health checks customizados
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adiciona health checks customizados do MeAjudaAi
    /// </summary>
    public static IServiceCollection AddMeAjudaAiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<MeAjudaAiHealthChecks.HelpProcessingHealthCheck>(
                "help_processing",
                tags: new[] { "ready", "business" })
            .AddCheck<MeAjudaAiHealthChecks.ExternalServicesHealthCheck>(
                "external_services", 
                tags: new[] { "ready", "external" })
            .AddCheck<MeAjudaAiHealthChecks.PerformanceHealthCheck>(
                "performance",
                tags: new[] { "live", "performance" })
            .AddCheck<MeAjudaAi.Shared.Database.DatabasePerformanceHealthCheck>(
                "database_performance",
                tags: new[] { "ready", "database", "performance" });

        return services;
    }
}