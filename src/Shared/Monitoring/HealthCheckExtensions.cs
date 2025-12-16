using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Extension methods para registrar health checks customizados
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adiciona health checks customizados do MeAjudaAi
    /// </summary>
    public static IServiceCollection AddMeAjudaAiHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Obter connection string do PostgreSQL
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string not found");

        // Registrar HttpClient para ExternalServicesHealthCheck com timeout
        services.AddHttpClient<MeAjudaAiHealthChecks.ExternalServicesHealthCheck>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5); // Health checks devem ser rápidos
            });

        // Registrar health checks
        var healthChecksBuilder = services.AddHealthChecks();
        
        healthChecksBuilder.AddCheck<MeAjudaAiHealthChecks.HelpProcessingHealthCheck>(
            "help_processing",
            tags: new[] { "ready", "business" });
            
        healthChecksBuilder.AddTypeActivatedCheck<MeAjudaAiHealthChecks.DatabasePerformanceHealthCheck>(
            "database_performance",
            failureStatus: null,
            tags: new[] { "ready", "database", "performance" },
            args: new object[] { connectionString });
            
        healthChecksBuilder.AddCheck<MeAjudaAiHealthChecks.ExternalServicesHealthCheck>(
            "external_services",
            tags: new[] { "ready", "external" });

        // Adicionar Hangfire health check
        // Monitora se o sistema de background jobs está operacional
        // CRÍTICO: Validação de compatibilidade Npgsql 10.x (Issue #39)
        healthChecksBuilder.AddCheck<MeAjudaAiHealthChecks.HangfireHealthCheck>(
            "hangfire",
            failureStatus: HealthStatus.Degraded, // Degraded em vez de Unhealthy permite app continuar funcionando
            tags: new[] { "ready", "background_jobs" });

        // Adicionar Redis health check se configurado
        var redisConnectionString = configuration["Caching:RedisConnectionString"];
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            healthChecksBuilder.AddRedis(
                redisConnectionString,
                name: "redis",
                tags: new[] { "ready", "cache" });
        }

        return services;
    }
}
