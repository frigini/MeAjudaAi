using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Queries;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
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
    public static IServiceCollection AddSearchProvidersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddEventHandlers();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString) && !MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
        {
            throw new InvalidOperationException(
                "Database connection string is not configured. " +
                "Please set one of the following configuration keys: " +
                "'ConnectionStrings:DefaultConnection', 'ConnectionStrings:Search', or 'ConnectionStrings:meajudaai-db'");
        }

        // DbContext principal para escrita/comandos (EF Core)
        services.AddDbContext<SearchProvidersDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "search_providers");
                    npgsqlOptions.EnableRetryOnFailure(3);
                });
            }

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Unit of Work
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.SearchProviders, (sp, key) => sp.GetRequiredService<SearchProvidersDbContext>());

        // Queries
        services.AddScoped<ISearchableProviderQueries, DbContextSearchableProviderQueries>();

        return services;
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo SearchProviders.
    /// </summary>
    private static IServiceCollection AddEventHandlers(this IServiceCollection services)
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

        return services;
    }
}



