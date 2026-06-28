using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços de infraestrutura do módulo ServiceCatalogs.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Extensions
{
    /// <summary>
    /// Registra todos os serviços de infraestrutura do módulo ServiceCatalogs.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddEventHandlers();

        services.ConfigureSchemaIsolation(configuration, ModuleNames.ServiceCatalogs, Schemas.ServiceCatalogs, DatabaseRoleConstants.ServiceCatalogs);

        return services;
    }

    /// <summary>
    /// Configura a persistência do banco de dados e repositórios do módulo.
    /// </summary>
    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<ServiceCatalogsDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration["Postgres:ConnectionString"]
                                  ?? configuration.GetConnectionString("DefaultConnection")
                                  ?? configuration.GetConnectionString("ServiceCatalogs")
                                  ?? configuration.GetConnectionString("meajudaai-db");

            if (string.IsNullOrEmpty(connectionString))
            {
                if (EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
                {
                    // Fallback para testes/dev quando a string de conexão não é crítica na inicialização do DI
                    connectionString = DatabaseConstants.DefaultTestConnectionString;
                }
                else
                {
                    throw new InvalidOperationException(
                        "PostgreSQL connection string not found. Configure connection string via Aspire, 'Postgres:ConnectionString' in appsettings.json, or as ConnectionStrings:meajudaai-db");
                }
            }

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ServiceCatalogsDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.ServiceCatalogs);
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

        // Processador de eventos de domínio
        services.AddScoped<IDomainEventProcessor, DomainEventProcessor>();

        // Unit of Work e Repositórios
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.ServiceCatalogs, (sp, key) => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ServiceCatalogsDbContext>());

        services.AddScoped<IRepository<Service, ServiceId>>(sp => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddScoped<IRepository<ServiceCategory, ServiceCategoryId>>(sp => sp.GetRequiredService<ServiceCatalogsDbContext>());

        // Consultas otimizadas
        services.AddScoped<IServiceCategoryQueries, DbContextServiceCategoryQueries>();
        services.AddScoped<IServiceQueries, DbContextServiceQueries>();

        // Command Handlers
        services.AddScoped<ICommandHandler<CreateServiceCategoryCommand, Result<ServiceCategoryDto>>, CreateServiceCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<CreateServiceCommand, Result<ServiceDto>>, CreateServiceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateServiceCategoryCommand, Result>, UpdateServiceCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateServiceCommand, Result>, UpdateServiceCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteServiceCategoryCommand, Result>, DeleteServiceCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteServiceCommand, Result>, DeleteServiceCommandHandler>();
        services.AddScoped<ICommandHandler<ActivateServiceCategoryCommand, Result>, ActivateServiceCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<ActivateServiceCommand, Result>, ActivateServiceCommandHandler>();
        services.AddScoped<ICommandHandler<DeactivateServiceCategoryCommand, Result>, DeactivateServiceCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<DeactivateServiceCommand, Result>, DeactivateServiceCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeServiceCategoryCommand, Result>, ChangeServiceCategoryCommandHandler>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetAllServiceCategoriesQuery, Result<IReadOnlyList<ServiceCategoryDto>>>, GetAllServiceCategoriesQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceCategoryByIdQuery, Result<ServiceCategoryDto?>>, GetServiceCategoryByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceCategoriesWithCountQuery, Result<IReadOnlyList<ServiceCategoryWithCountDto>>>, GetServiceCategoriesWithCountQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllServicesQuery, Result<IReadOnlyList<ServiceListDto>>>, GetAllServicesQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceByIdQuery, Result<ServiceDto?>>, GetServiceByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetServicesByCategoryQuery, Result<IReadOnlyList<ServiceListDto>>>, GetServicesByCategoryQueryHandler>();
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo ServiceCatalogs.
    /// </summary>
    private static void AddEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<IEventHandler<ServiceActivatedDomainEvent>, ServiceActivatedDomainEventHandler>();
        services.AddScoped<IEventHandler<ServiceDeactivatedDomainEvent>, ServiceDeactivatedDomainEventHandler>();
        services.AddScoped<IEventHandler<ServiceUpdatedDomainEvent>, ServiceUpdatedDomainEventHandler>();
    }
}
