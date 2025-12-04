using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços da camada de Infrastructure do SearchProviders.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registra serviços da camada de Infrastructure do SearchProviders.
    /// </summary>
    /// <param name="services">A coleção de serviços.</param>
    /// <param name="configuration">A configuração para ler strings de conexão e configurações.</param>
    /// <param name="environment">O ambiente de hospedagem para determinar comportamento em Testing.</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddSearchProvidersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Registrar DbContext com suporte PostGIS
        // IMPORTANTE: EF Core usa GetConnectionString("DefaultConnection") enquanto Dapper (via PostgresOptions)
        // resolve de "Postgres:ConnectionString" ou "ConnectionStrings:meajudaai-db".
        // Certifique-se de que estas chaves de configuração apontem para o mesmo database para evitar que EF e Dapper
        // se comuniquem com databases/schemas diferentes entre ambientes (dev/test/prod, Aspire, etc.).
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                              ?? configuration.GetConnectionString("Search")
                              ?? configuration.GetConnectionString("meajudaai-db");

        // Em ambiente de teste, permitir inicialização sem connection string
        // (útil para testes unitários que não acessam o banco)
        var isTesting = environment.IsEnvironment("Testing") 
                     || string.Equals(Environment.GetEnvironmentVariable("INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase);
        
        if (string.IsNullOrEmpty(connectionString) && !isTesting)
        {
            throw new InvalidOperationException(
                "Database connection string not found. Tried: 'DefaultConnection', 'Search', 'meajudaai-db'. " +
                "Please configure one of these connection strings in appsettings.json or environment variables.");
        }

        // Só registrar DbContext se houver connection string
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<SearchProvidersDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "meajudaai_searchproviders");
                    npgsqlOptions.UseNetTopologySuite(); // Habilitar suporte PostGIS/geoespacial
                });

                options.UseSnakeCaseNamingConvention();

                // Habilitar erros detalhados em desenvolvimento
                if (configuration.GetValue<bool>("DetailedErrors"))
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            // Registrar Dapper para queries espaciais otimizadas
            services.AddDapper();

            // Registrar repositórios
            services.AddScoped<ISearchableProviderRepository, SearchableProviderRepository>();
        }
        
        // Em ambiente de teste sem connection string, registrar mock ou skip
        // Os testes que precisarem de DbContext devem configurar explicitamente

        // Registrar Event Handlers (sempre necessário, independente de connection string)
        services.AddEventHandlers();

        return services;
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo SearchProviders.
    /// </summary>
    private static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        // Integration Event Handlers
        services.AddScoped<IEventHandler<ProviderActivatedIntegrationEvent>, ProviderActivatedIntegrationEventHandler>();

        return services;
    }
}
