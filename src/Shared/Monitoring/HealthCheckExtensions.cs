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

        // NOTA: ServiceDefaults já registra ExternalServicesHealthCheck, não precisamos registrar novamente aqui

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

        // NOTA: ExternalServicesHealthCheck é registrado pelo ServiceDefaults, não precisamos registrar aqui
            
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
