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

    private static void AddDatabaseHealthCheck(this IServiceCollection services)
    {
        // Em ambiente de teste, adiciona um health check mock ao invés do real
        if (IsTestingEnvironment())
        {
            services.AddHealthChecks()
                .AddCheck("postgres", () => HealthCheckResult.Healthy("Database ready for testing"), ["ready", "database"]);
            return;
        }

        // Registra PostgresOptions como singleton para PostgresHealthCheck
        services.AddSingleton<PostgresOptions>(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value);

        // Registra o health check do Postgres
        services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready", "database"]);
    }

    private static void AddExternalServicesHealthCheck(this IServiceCollection services)
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
        services.AddHealthChecks()
            .AddCheck<ExternalServicesHealthCheck>("external-services", tags: ["ready", "external"]);
    }

    private static void AddCacheHealthCheck(this IServiceCollection services)
    {
        // Health check simples para cache
        services.AddHealthChecks()
            .AddCheck("cache", () => HealthCheckResult.Healthy("Cache is available"), ["ready", "cache"]);
    }

    /// <summary>
    /// Determina se o ambiente atual é de Teste, usando a mesma precedência do EnvironmentHelpers do AppHost
    /// </summary>
    private static bool IsTestingEnvironment()
    {
        // Verifica DOTNET_ENVIRONMENT primeiro, depois ASPNETCORE_ENVIRONMENT (mesma precedência do EnvironmentHelpers)
        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var envName = !string.IsNullOrEmpty(dotnetEnv) ? dotnetEnv : aspnetEnv;

        if (!string.IsNullOrEmpty(envName) &&
            string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Verifica a variável de ambiente INTEGRATION_TESTS com parse booleano robusto
        var integrationTestsValue = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        if (!string.IsNullOrEmpty(integrationTestsValue))
        {
            // Lida com padrões "true"/"false" e "1"/"0" de forma insensível a maiúsculas/minúsculas
            if (bool.TryParse(integrationTestsValue, out var boolResult))
            {
                return boolResult;
            }

            // Lida com "1" como verdadeiro (comum em ambientes de CI/CD)
            if (string.Equals(integrationTestsValue, "1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
