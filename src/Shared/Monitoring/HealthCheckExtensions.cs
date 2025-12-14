using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        // Registrar HttpClient para ExternalServicesHealthCheck
        services.AddHttpClient<MeAjudaAiHealthChecks.ExternalServicesHealthCheck>();

        // Registrar health checks
        var healthChecksBuilder = services.AddHealthChecks();
        
        healthChecksBuilder.AddCheck<MeAjudaAiHealthChecks.HelpProcessingHealthCheck>(
            "help_processing",
            tags: ["ready", "business"]);
            
        healthChecksBuilder.AddTypeActivatedCheck<MeAjudaAiHealthChecks.DatabasePerformanceHealthCheck>(
            "database_performance",
            failureStatus: null,
            tags: new[] { "ready", "database", "performance" },
            args: new object[] { connectionString });
            
        healthChecksBuilder.AddCheck<MeAjudaAiHealthChecks.ExternalServicesHealthCheck>(
            "external_services",
            tags: ["ready", "external"]);

        return services;
    }
}
