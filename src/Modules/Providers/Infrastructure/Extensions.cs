using MeAjudaAi.Modules.Providers.Application.Services.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            // Suprimir o warning PendingModelChangesWarning apenas em ambiente de desenvolvimento
            var environment = serviceProvider.GetService<IHostEnvironment>();
            if (environment?.IsDevelopment() == true)
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

        // Registro do repositório
        services.AddScoped<IProviderRepository, ProviderRepository>();

        // Registro do serviço de consultas
        services.AddScoped<IProviderQueryService, ProviderQueryService>();

        // Registro dos Event Handlers
        services.AddEventHandlers();

        return services;
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo Providers.
    /// </summary>
    private static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        // Domain Event Handlers
        services.AddScoped<IEventHandler<ProviderRegisteredDomainEvent>, ProviderRegisteredDomainEventHandler>();
        services.AddScoped<IEventHandler<ProviderDeletedDomainEvent>, ProviderDeletedDomainEventHandler>();
        services.AddScoped<IEventHandler<ProviderVerificationStatusUpdatedDomainEvent>, ProviderVerificationStatusUpdatedDomainEventHandler>();
        services.AddScoped<IEventHandler<ProviderProfileUpdatedDomainEvent>, ProviderProfileUpdatedDomainEventHandler>();

        // Integration Event Handlers
        services.AddScoped<IEventHandler<DocumentVerifiedIntegrationEvent>, DocumentVerifiedIntegrationEventHandler>();

        return services;
    }
}
