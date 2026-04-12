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

using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;

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
        // Só fazemos bypass se estivermos explicitamente em Desenvolvimento e NÃO for um live environment
        var isTesting = MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment(environment);

        if (string.IsNullOrEmpty(connectionString))
        {
            if (isTesting)
            {
#pragma warning disable S2068 // "password" detected here, make sure this is not a hard-coded credential
                connectionString = MeAjudaAi.Shared.Database.DatabaseConstants.DefaultTestConnectionString;
#pragma warning restore S2068
            }
            else
            {
                var env1 = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var env2 = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
                var env3 = environment?.EnvironmentName;
                throw new InvalidOperationException(
                    $"DEBUG: isTesting={isTesting}, ASPNETCORE={env1}, INTEGRATION_TESTS={env2}, EnvName={env3} " +
                    "Database connection string not found. Tried: 'DefaultConnection', 'Search', 'meajudaai-db'.");
            }
        }

        // Sempre registrar DbContext (mesmo que connection string seja vazia em testes unitários)
        // Em E2E tests, a connection string será fornecida via configuração
        services.AddDbContext<SearchProvidersDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "search_providers");
                npgsqlOptions.UseNetTopologySuite(); // Habilitar suporte PostGIS/geoespacial
            });

            options.UseSnakeCaseNamingConvention();

            // Suprimir o warning PendingModelChangesWarning apenas em ambiente de desenvolvimento
            if (environment.IsDevelopment())
            {
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }

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
        services.AddScoped<IEventHandler<ProviderServicesUpdatedIntegrationEvent>, ProviderServicesUpdatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ServiceDeactivatedIntegrationEvent>, ServiceDeactivatedIntegrationEventHandler>();

        return services;
    }
}
