using MeAjudaAi.Modules.Providers.Application.Services;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.HealthChecks;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeAjudaAi.Modules.Providers.Infrastructure;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços de infraestrutura do módulo Providers.
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuração do DbContext - mais tolerante para ambientes de teste
        services.AddDbContext<ProvidersDbContext>((serviceProvider, options) =>
        {
            // Usa PostgreSQL para todos os ambientes (TestContainers fornecerá database de teste)
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                  ?? configuration.GetConnectionString("Providers")
                                  ?? configuration.GetConnectionString("meajudaai-db");

            // Em ambientes de teste, permitir que seja configurado depois via override
            var isTestEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing" ||
                                   Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Testing";

            if (string.IsNullOrEmpty(connectionString))
            {
                if (isTestEnvironment)
                {
                    // Para testes, usar uma connection string temporária que será substituída
                    connectionString = "Host=localhost;Database=temp_test;Username=postgres;Password=test";
                }
                else
                {
                    throw new InvalidOperationException(
                        "Connection string not found in configuration. " +
                        "Please ensure a connection string is properly configured in appsettings.json or environment variables " +
                        "with name 'DefaultConnection', 'Providers', or 'meajudaai-db'.");
                }
            }

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ProvidersDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "providers");

                // PERFORMANCE: Timeout mais longo para permitir criação do banco de dados
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            // Configurações consistentes para evitar problemas com compiled queries
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);
        });

        // Registro do repositório
        services.AddScoped<IProviderRepository, ProviderRepository>();

        // Registro do serviço de consultas
        services.AddScoped<IProviderQueryService, ProviderQueryService>();

        // Adiciona health check específico para o módulo Providers
        services.AddHealthChecks()
            .AddCheck<ProvidersHealthCheck>("providers", 
                tags: ["ready", "database", "providers"]);

        // Registra o health check customizado
        services.AddScoped<ProvidersHealthCheck>();

        return services;
    }
}
