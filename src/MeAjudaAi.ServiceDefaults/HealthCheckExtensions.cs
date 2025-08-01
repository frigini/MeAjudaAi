using MeAjudaAi.ServiceDefaults.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.ServiceDefaults;

public static class HealthCheckExtensions
{
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        builder.Services.AddDatabaseHealthCheck();
        builder.Services.AddExternalServicesHealthCheck();
        builder.Services.AddCacheHealthCheck();

        return builder;
    }

    private static IHealthChecksBuilder AddDatabaseHealthCheck(this IServiceCollection services)
    {
        return services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready", "database"]);
    }

    private static IHealthChecksBuilder AddExternalServicesHealthCheck(
        this IServiceCollection services)
    {
        services.AddHttpClient<ExternalServicesHealthCheck>();

        return services.AddHealthChecks()
            .AddCheck<ExternalServicesHealthCheck>("external_services", tags: ["ready"]);
    }

    private static IHealthChecksBuilder AddCacheHealthCheck(
        this IServiceCollection services)
    {
        return services.AddHealthChecks();
    }
}