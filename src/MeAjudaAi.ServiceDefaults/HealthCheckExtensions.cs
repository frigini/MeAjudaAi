using MeAjudaAi.ServiceDefaults.HealthChecks;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Configuration;
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

        builder.Services.AddDatabaseHealthCheck(builder.Configuration);
        builder.Services.AddExternalServicesHealthCheck();
        builder.Services.AddCacheHealthCheck(builder.Configuration);

        return builder;
    }

    private static IHealthChecksBuilder AddDatabaseHealthCheck(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgresOptions = configuration
            .GetSection(PostgresOptions.SectionName)
            .Get<PostgresOptions>();

        if (!string.IsNullOrEmpty(postgresOptions?.ConnectionString))
        {
            return services.AddHealthChecks()
                .AddNpgSql(postgresOptions.ConnectionString, tags: ["ready", "database"]);
        }

        return services.AddHealthChecks();
    }

    private static IHealthChecksBuilder AddExternalServicesHealthCheck(
        this IServiceCollection services)
    {
        services.AddHttpClient<ExternalServicesHealthCheck>();

        return services.AddHealthChecks()
            .AddCheck<ExternalServicesHealthCheck>("external_services", tags: ["ready"]);
    }

    private static IHealthChecksBuilder AddCacheHealthCheck(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddHealthChecks();
    }
}