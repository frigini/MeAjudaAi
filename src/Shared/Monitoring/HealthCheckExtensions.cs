using MeAjudaAi.Shared.Jobs.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
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
        this IServiceCollection services)
    {
        // NOTA: ServiceDefaults já registra health checks de infraestrutura:
        // - PostgresHealthCheck (database)
        // - ExternalServicesHealthCheck (keycloak, etc)
        // - CacheHealthCheck (redis)
        
        // Aqui registramos apenas health checks específicos da aplicação
        var healthChecksBuilder = services.AddHealthChecks();
            
        // Adicionar Hangfire health check
        // Monitora se o sistema de background jobs está operacional
        // CRÍTICO: Validação de compatibilidade Npgsql 10.x (Issue #39)
        healthChecksBuilder.AddCheck<HangfireHealthCheck>(
            "hangfire",
            failureStatus: HealthStatus.Degraded, // Degraded em vez de Unhealthy permite app continuar funcionando
            tags: new[] { "ready", "background_jobs" });

        return services;
    }
}
