using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Monitoring;

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
                tags: ["ready", "business"])
            .AddCheck<MeAjudaAiHealthChecks.ExternalServicesHealthCheck>(
                "external_services",
                tags: ["ready", "external"])
            .AddCheck<MeAjudaAiHealthChecks.PerformanceHealthCheck>(
                "performance",
                tags: ["live", "performance"])
            .AddCheck<MeAjudaAi.Shared.Database.DatabasePerformanceHealthCheck>(
                "database_performance",
                tags: ["ready", "database", "performance"]);

        return services;
    }
}