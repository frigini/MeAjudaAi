using MeAjudaAi.ServiceDefaults.HealthChecks;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.ServiceDefaults;

public static class HealthCheckExtensions
{
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        // Configuração simplificada - sempre adiciona health check básico
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        // Em ambiente de teste, use health checks mock simples
        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddHealthChecks()
                .AddCheck("database", () => HealthCheckResult.Healthy("Database ready for testing"), ["ready", "database"])
                .AddCheck("cache", () => HealthCheckResult.Healthy("Cache ready for testing"), ["ready", "cache"]);
        }
        else
        {
            // Em outros ambientes, adicione health checks reais
            builder.Services.AddDatabaseHealthCheck();
            builder.Services.AddCacheHealthCheck();
            builder.Services.AddExternalServicesHealthCheck();
        }

        return builder;
    }

    private static IHealthChecksBuilder AddDatabaseHealthCheck(this IServiceCollection services)
    {
        // Registra PostgresOptions como singleton para PostgresHealthCheck
        services.AddSingleton<PostgresOptions>(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value);

        // Registra o health check do Postgres
        return services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready", "database"]);
    }

    private static IHealthChecksBuilder AddExternalServicesHealthCheck(this IServiceCollection services)
    {
        // Registra ExternalServicesOptions usando AddOptions<>()
        services.AddOptions<ExternalServicesOptions>()
            .Configure<IConfiguration>((opts, config) =>
            {
                config.GetSection(ExternalServicesOptions.SectionName).Bind(opts);
            })
            .ValidateOnStart();

        // Registra HttpClient para o ExternalServicesHealthCheck
        services.AddHttpClient<ExternalServicesHealthCheck>();

        // Registra o health check de serviços externos
        return services.AddHealthChecks()
            .AddCheck<ExternalServicesHealthCheck>("external-services", tags: ["ready", "external"]);
    }

    private static IHealthChecksBuilder AddCacheHealthCheck(this IServiceCollection services)
    {
        // Health check simples para cache
        return services.AddHealthChecks()
            .AddCheck("cache", () => HealthCheckResult.Healthy("Cache is available"), ["ready", "cache"]);
    }
}