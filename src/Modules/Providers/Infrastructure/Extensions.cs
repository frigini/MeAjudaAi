using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços de infraestrutura do módulo Providers.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Extensions
{
    /// <summary>
    /// Registra todos os serviços de infraestrutura do módulo Providers.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddEventHandlers();

        services.ConfigureSchemaIsolation(configuration, ModuleNames.Providers, Schemas.Providers, DatabaseRoleConstants.Providers);

        return services;
    }

    /// <summary>
    /// Configura a persistência do banco de dados e repositórios do módulo.
    /// </summary>
    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<ProvidersDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                  ?? configuration.GetConnectionString("Providers")
                                  ?? configuration.GetConnectionString("meajudaai-db");

            // Em ambiente de teste, permitir inicialização sem connection string
            var isTesting = EnvironmentHelpers.IsSecurityBypassEnvironment(environment);

            if (string.IsNullOrEmpty(connectionString))
            {
                if (isTesting)
                {
                    // Para testes, usar uma connection string temporária que será substituída
                    connectionString = DatabaseConstants.DefaultTestConnectionString;
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
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);

            // Suprimir o warning PendingModelChangesWarning apenas em ambiente de desenvolvimento
            if (environment.IsDevelopment())
            {
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }
        });

        // AUTO-MIGRATION: Configura factory para auto-aplicar migrations quando necessário
        services.AddScoped<Func<ProvidersDbContext>>(provider => () =>
        {
            var context = provider.GetRequiredService<ProvidersDbContext>();
            // Aplica migrações pendentes - ABORDAGEM PREGUIÇOSA
            context.Database.Migrate();
            return context;
        });

        // Registro do processador de eventos de domínio
        services.AddScoped<IDomainEventProcessor, DomainEventProcessor>();

        // Unit of Work e Repositórios
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProvidersDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Providers, (sp, key) => sp.GetRequiredService<ProvidersDbContext>());

        services.AddScoped<IIdempotencyRepository>(sp => new ProviderIdempotencyRepository(sp.GetRequiredService<ProvidersDbContext>()));
        
        services.AddScoped<IRepository<Provider, Guid>>(sp => sp.GetRequiredService<ProvidersDbContext>());

        // Consultas otimizadas
        services.AddScoped<IProviderQueries, DbContextProviderQueries>();
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo Providers.
    /// </summary>
    private static void AddEventHandlers(this IServiceCollection services)
    {
        // Domain Event Handlers
        services.AddScoped<IEventHandler<ProviderRegisteredDomainEvent>, ProviderRegisteredDomainEventHandler>();
        services.AddScoped<IEventHandler<ProviderDeletedDomainEvent>, ProviderDeletedDomainEventHandler>();
        services.AddScoped<IEventHandler<ProviderVerificationStatusUpdatedDomainEvent>, ProviderVerificationStatusUpdatedDomainEventHandler>();
        services.AddScoped<IEventHandler<ProviderProfileUpdatedDomainEvent>, ProviderProfileUpdatedDomainEventHandler>();
        services.AddScoped<IEventHandler<ProviderActivatedDomainEvent>, ProviderActivatedDomainEventHandler>();
        services.AddScoped<IEventHandler<ProviderAwaitingVerificationDomainEvent>, ProviderAwaitingVerificationDomainEventHandler>();
        services.AddScoped<IEventHandler<ProviderServiceAddedDomainEvent>, ProviderServiceAddedDomainEventHandler>();
        services.AddScoped<IEventHandler<ProviderServiceRemovedDomainEvent>, ProviderServiceRemovedDomainEventHandler>();

        // Integration Event Handlers
        services.AddScoped<IEventHandler<DocumentVerifiedIntegrationEvent>, DocumentVerifiedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<SubscriptionActivatedIntegrationEvent>, SubscriptionActivatedIntegrationEventHandler>();
        services.AddScoped<IEventHandler<SubscriptionCanceledIntegrationEvent>, SubscriptionCanceledIntegrationEventHandler>();
        services.AddScoped<IEventHandler<SubscriptionExpiredIntegrationEvent>, SubscriptionExpiredIntegrationEventHandler>();
        services.AddScoped<IEventHandler<ServiceNameUpdatedIntegrationEvent>, ServiceNameUpdatedIntegrationEventHandler>();
    }
}

