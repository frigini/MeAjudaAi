using MeAjudaAi.Shared.Tests.Mocks.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Configurações de infraestrutura mock para testes
/// Centraliza configurações de logging, database, messaging, cache, etc.
/// </summary>
public static class MockInfrastructureExtensions
{
    /// <summary>
    /// Adiciona configurações otimizadas de logging para testes
    /// Reduz verbosidade mantendo apenas informações essenciais
    /// </summary>
    public static IServiceCollection AddMockLogging(this IServiceCollection services)
    {
        services.Configure<LoggerFilterOptions>(options =>
        {
            options.MinLevel = LogLevel.Warning; // Apenas Warning e Error

            // Específicos para Entity Framework (muito verboso)
            options.Rules.Add(new LoggerFilterRule(null, "Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Error, null));
            options.Rules.Add(new LoggerFilterRule(null, "Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Error, null));
            options.Rules.Add(new LoggerFilterRule(null, "Microsoft.EntityFrameworkCore.Migrations", LogLevel.Warning, null));

            // Específicos para ASP.NET Core
            options.Rules.Add(new LoggerFilterRule(null, "Microsoft.AspNetCore.Hosting", LogLevel.Warning, null));
            options.Rules.Add(new LoggerFilterRule(null, "Microsoft.AspNetCore.Routing", LogLevel.Error, null));
            options.Rules.Add(new LoggerFilterRule(null, "Microsoft.AspNetCore.Authentication", LogLevel.Warning, null));

            // Específicos para HTTP Client
            options.Rules.Add(new LoggerFilterRule(null, "System.Net.Http.HttpClient", LogLevel.Error, null));

            // TestContainers (apenas erros críticos)
            options.Rules.Add(new LoggerFilterRule(null, "Testcontainers", LogLevel.Error, null));
        });

        return services;
    }

    /// <summary>
    /// Adiciona configurações padrão para testes
    /// Sobrepõe configurações que podem interferir com testes
    /// </summary>
    public static void AddTestConfiguration(this IConfigurationBuilder config)
    {
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Logging
            ["Logging:LogLevel:Default"] = "Warning",
            ["Logging:LogLevel:Microsoft"] = "Warning",
            ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
            ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning",
            ["Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command"] = "Error",
            ["Logging:LogLevel:Microsoft.EntityFrameworkCore.Infrastructure"] = "Error",
            ["Logging:LogLevel:System.Net.Http.HttpClient"] = "Error",

            // Desabilita features desnecessárias em testes
            ["HealthChecks:EnableDetailedErrors"] = "false",
            ["Metrics:Enabled"] = "false",
            ["OpenTelemetry:Enabled"] = "false",

            // Timeouts otimizados para testes
            ["HttpClient:Timeout"] = "00:00:30",
            ["Database:CommandTimeout"] = "30",

            // Desabilita caches que podem interferir
            ["ResponseCaching:Enabled"] = "false",
            ["OutputCaching:Enabled"] = "false"
        });
    }

    /// <summary>
    /// Remove serviços que podem interferir com testes
    /// </summary>
    public static IServiceCollection RemoveProductionServices(this IServiceCollection services)
    {
        // Remove TODOS os serviços do namespace MeAjudaAi.Shared.Caching que podem causar problemas
        var cacheServices = services.Where(s =>
            s.ServiceType.FullName?.StartsWith("MeAjudaAi.Shared.Caching") == true ||
            s.ImplementationType?.FullName?.StartsWith("MeAjudaAi.Shared.Caching") == true
        ).ToList();

        // Remove também behaviors que dependem de cache
        var cachingBehaviors = services.Where(s =>
            s.ServiceType.FullName?.Contains("MeAjudaAi.Shared.Behaviors.CachingBehavior") == true ||
            s.ImplementationType?.FullName?.Contains("MeAjudaAi.Shared.Behaviors.CachingBehavior") == true
        ).ToList();

        // Remove authentication handlers existentes para evitar conflitos
        var authHandlers = services.Where(s =>
            s.ServiceType.FullName?.Contains("Microsoft.AspNetCore.Authentication.IAuthenticationHandler") == true ||
            s.ServiceType.Name.Contains("AuthenticationHandler") ||
            s.ServiceType == typeof(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider)
        ).ToList();

        var allServicesToRemove = cacheServices.Concat(cachingBehaviors).Concat(authHandlers).ToList();

        foreach (var service in allServicesToRemove)
        {
            services.Remove(service);
        }

        // Cache services removidos para testes

        return services;
    }
}

/// <summary>
/// Configurações específicas para diferentes tipos de teste
/// </summary>
public static class TestEnvironmentProfiles
{
    /// <summary>
    /// Configuração para testes unitários (mais leve)
    /// </summary>
    public static void ConfigureForUnitTests(IServiceCollection services)
    {
        services.AddMockLogging();
        services.RemoveProductionServices();

        // Configurações específicas para unit tests
        services.Configure<LoggerFilterOptions>(options =>
        {
            options.MinLevel = LogLevel.Error; // Apenas erros em unit tests
        });
    }

    /// <summary>
    /// Configuração para testes de integração (balanceada)
    /// </summary>
    public static void ConfigureForIntegrationTests(IServiceCollection services)
    {
        services.AddMockLogging();
        services.RemoveProductionServices();

        // Add messaging mocks for integration tests
        services.AddMessagingMocks();

        // NOTE: Authentication will be configured separately to avoid conflicts

        // Force reconfigure PostgresOptions to use test configuration
        // Remove existing PostgresOptions and reconfigure with test priority
        var existingOptions = services.FirstOrDefault(s => s.ServiceType == typeof(MeAjudaAi.Shared.Database.PostgresOptions));
        if (existingOptions != null)
        {
            services.Remove(existingOptions);
        }

        // Re-add with test configuration priority
        services.AddOptions<MeAjudaAi.Shared.Database.PostgresOptions>()
            .Configure<IConfiguration>((opts, config) =>
            {
                opts.ConnectionString =
                    config.GetConnectionString("DefaultConnection") ??      // TestContainer connection (highest priority)
                    config.GetConnectionString("meajudaai-db-local") ??
                    config.GetConnectionString("meajudaai-db") ??
                    config["Postgres:ConnectionString"] ??
                    string.Empty;
            });

        // Permite warnings importantes em integration tests
        services.Configure<LoggerFilterOptions>(options =>
        {
            options.MinLevel = LogLevel.Warning;
        });
    }

    /// <summary>
    /// Configuração para testes E2E (mais verboso para debugging)
    /// </summary>
    public static void ConfigureForE2ETests(IServiceCollection services)
    {
        services.AddMockLogging();

        // E2E tests podem precisar de mais informações
        services.Configure<LoggerFilterOptions>(options =>
        {
            options.MinLevel = LogLevel.Information;

            // Mas ainda silencia EF Core
            options.Rules.Add(new LoggerFilterRule(null, "Microsoft.EntityFrameworkCore", LogLevel.Warning, null));
        });
    }
}

/// <summary>
/// Helper para detectar tipo de teste automaticamente
/// </summary>
public static class TestTypeDetector
{
    public static TestType DetectTestType()
    {
        var testAssembly = System.Reflection.Assembly.GetCallingAssembly().GetName().Name;

        return testAssembly switch
        {
            var name when name?.Contains("Unit") == true => TestType.Unit,
            var name when name?.Contains("Integration") == true => TestType.Integration,
            var name when name?.Contains("E2E") == true => TestType.E2E,
            _ => TestType.Integration // Default
        };
    }

    public static void ConfigureServicesForTestType(IServiceCollection services)
    {
        var testType = DetectTestType();

        switch (testType)
        {
            case TestType.Unit:
                TestEnvironmentProfiles.ConfigureForUnitTests(services);
                break;
            case TestType.Integration:
                TestEnvironmentProfiles.ConfigureForIntegrationTests(services);
                break;
            case TestType.E2E:
                TestEnvironmentProfiles.ConfigureForE2ETests(services);
                break;
        }
    }
}

public enum TestType
{
    Unit,
    Integration,
    E2E
}
