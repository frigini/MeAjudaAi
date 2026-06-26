using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Queries;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços de infraestrutura do módulo SearchProviders.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Extensions
{
    /// <summary>
    /// Registra todos os serviços de infraestrutura do módulo SearchProviders.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddEventHandlers();

        services.ConfigureSchemaIsolation(configuration, ModuleNames.SearchProviders, Schemas.SearchProviders, DatabaseRoleConstants.SearchProviders);

        return services;
    }

    /// <summary>
    /// Configura a persistência do banco de dados e repositórios do módulo.
    /// </summary>
    private static void AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration.GetConnectionString("SearchProviders")
            ?? configuration.GetConnectionString("meajudaai-db");

        if (string.IsNullOrWhiteSpace(connectionString) && EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
        {
            connectionString = DatabaseConstants.DefaultTestConnectionString;
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Configure ConnectionStrings:DefaultConnection, SearchProviders, or meajudaai-db.");
        }

        // DbContext principal para escrita/comandos (EF Core)
        services.AddDbContext<SearchProvidersDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(SearchProvidersDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.SearchProviders);
                npgsqlOptions.CommandTimeout(60);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                npgsqlOptions.UseNetTopologySuite();
            })
            .UseSnakeCaseNamingConvention()
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);

            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }
        });

        // Unit of Work e Repositórios
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.SearchProviders, (sp, key) => sp.GetRequiredService<SearchProvidersDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SearchProvidersDbContext>());

        services.AddScoped<IRepository<SearchableProvider, SearchableProviderId>>(sp => sp.GetRequiredService<SearchProvidersDbContext>());

        // Consultas otimizadas
        services.AddScoped<ISearchableProviderQueries, DbContextSearchableProviderQueries>();
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo SearchProviders.
    /// </summary>
    private static void AddEventHandlers(this IServiceCollection services)
    {
        // Integration Event Handlers
        services.AddScoped<IEventHandler<ProviderActivatedIntegrationEvent>, ProviderActivatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ProviderProfileUpdatedIntegrationEvent>, ProviderProfileUpdatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ProviderDeletedIntegrationEvent>, ProviderDeletedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ProviderIndexRequiredIntegrationEvent>, ProviderIndexRequiredIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ProviderServicesUpdatedIntegrationEvent>, ProviderServicesUpdatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ServiceDeactivatedIntegrationEvent>, ServiceDeactivatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ServiceActivatedIntegrationEvent>, ServiceActivatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<AllowedCityCreatedIntegrationEvent>, AllowedCityCreatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<AllowedCityDeletedIntegrationEvent>, AllowedCityDeletedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<AllowedCityUpdatedIntegrationEvent>, AllowedCityUpdatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ReviewApprovedIntegrationEvent>, ReviewApprovedIntegrationEventHandler>();
    }
}
