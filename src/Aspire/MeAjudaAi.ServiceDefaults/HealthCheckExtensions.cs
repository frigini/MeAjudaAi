using MeAjudaAi.ServiceDefaults.HealthChecks;
using MeAjudaAi.ServiceDefaults.Options;
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
        // Em ambiente de teste, use APENAS health checks mock simples
        if (IsTestingEnvironment())
        {
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("Self check passed for testing"), ["live"])
                .AddCheck("database", () => HealthCheckResult.Healthy("Database ready for testing"), ["ready", "database"])
                .AddCheck("cache", () => HealthCheckResult.Healthy("Cache ready for testing"), ["ready", "cache"])
                .AddCheck("external-services", () => HealthCheckResult.Healthy("External services ready for testing"), ["ready", "external"]);
        }
        else
        {
            // Configuração normal para outros ambientes
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            // Em outros ambientes, adicione health checks reais
            builder.Services.AddDatabaseHealthCheck();
            builder.Services.AddCacheHealthCheck();
            builder.Services.AddExternalServicesHealthCheck();
        }

        return builder;
    }

    private static IHealthChecksBuilder AddDatabaseHealthCheck(this IServiceCollection services)
    {
        // Em ambiente de teste, adiciona um health check mock ao invés do real
        if (IsTestingEnvironment())
        {
            return services.AddHealthChecks()
                .AddCheck("postgres", () => HealthCheckResult.Healthy("Database ready for testing"), ["ready", "database"]);
        }

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
            .BindConfiguration(ExternalServicesOptions.SectionName)
            .ValidateOnStart();

        // Registra ExternalServicesOptions como singleton para DI direto
        services.AddSingleton<ExternalServicesOptions>(sp =>
            sp.GetRequiredService<IOptions<ExternalServicesOptions>>().Value);

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

    /// <summary>
    /// Determines if the current environment is Testing using the same precedence as AppHost EnvironmentHelpers
    /// </summary>
    private static bool IsTestingEnvironment()
    {
        // Check DOTNET_ENVIRONMENT first, then fallback to ASPNETCORE_ENVIRONMENT (same precedence as EnvironmentHelpers)
        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var envName = !string.IsNullOrEmpty(dotnetEnv) ? dotnetEnv : aspnetEnv;

        if (!string.IsNullOrEmpty(envName) &&
            string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check INTEGRATION_TESTS environment variable with robust boolean parsing
        var integrationTestsValue = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        if (!string.IsNullOrEmpty(integrationTestsValue))
        {
            // Handle both "true"/"false" and "1"/"0" patterns case-insensitively
            if (bool.TryParse(integrationTestsValue, out var boolResult))
            {
                return boolResult;
            }

            // Handle "1" as true (common in CI/CD environments)
            if (string.Equals(integrationTestsValue, "1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
