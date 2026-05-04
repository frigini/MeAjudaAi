using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços de infraestrutura do módulo ServiceCatalogs.
    /// </summary>
    public static IServiceCollection AddServiceCatalogsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ... rest of DB configuration ...

        services.AddDbContext<ServiceCatalogsDbContext>((serviceProvider, options) =>
        {
            var environment = serviceProvider.GetService<IHostEnvironment>();
            var isTestEnvironment = environment?.EnvironmentName == "Testing";

            // Use shared connection string resolution logic (same precedence as DapperConnection)
            var connectionString = configuration["Postgres:ConnectionString"]
                                  ?? configuration.GetConnectionString("DefaultConnection")
                                  ?? configuration.GetConnectionString("ServiceCatalogs")
                                  ?? configuration.GetConnectionString("meajudaai-db");

            if (string.IsNullOrEmpty(connectionString))
            {
                if (MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment())
                {
                    // Fallback para testes/dev quando a string de conexão não é crítica na inicialização do DI
#pragma warning disable S2068 // "password" detected here, make sure this is not a hard-coded credential
                    connectionString = MeAjudaAi.Shared.Database.DatabaseConstants.DefaultTestConnectionString;
#pragma warning restore S2068
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
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "service_catalogs");
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);

            // Suprimir o warning PendingModelChangesWarning apenas em ambiente de desenvolvimento
            if (environment?.IsDevelopment() == true)
            {
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddScoped<MeAjudaAi.Shared.Database.IRepository<Domain.Entities.ServiceCategory, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>>(sp => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddScoped<MeAjudaAi.Shared.Database.IRepository<Domain.Entities.Service, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>>(sp => sp.GetRequiredService<ServiceCatalogsDbContext>());

        // Registra o QueryDispatcher do Shared
        services.AddScoped<MeAjudaAi.Shared.Queries.IQueryDispatcher, MeAjudaAi.Shared.Queries.QueryDispatcher>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandDispatcher, MeAjudaAi.Shared.Commands.CommandDispatcher>();
        
        // Registra queries services
        services.AddScoped<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.IServiceQueries, DbContextServiceQueries>();
        services.AddScoped<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.IServiceCategoryQueries, DbContextServiceCategoryQueries>();

        services.AddScoped<MeAjudaAi.Contracts.Modules.ServiceCatalogs.IServiceCatalogsModuleApi, MeAjudaAi.Modules.ServiceCatalogs.Application.ModuleApi.ServiceCatalogsModuleApi>();

        // Registra command handlers
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

        // Registra query handlers
        services.AddScoped<IQueryHandler<GetAllServiceCategoriesQuery, Result<IReadOnlyList<ServiceCategoryDto>>>, GetAllServiceCategoriesQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceCategoryByIdQuery, Result<ServiceCategoryDto?>>, GetServiceCategoryByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceCategoriesWithCountQuery, Result<IReadOnlyList<ServiceCategoryWithCountDto>>>, GetServiceCategoriesWithCountQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllServicesQuery, Result<IReadOnlyList<ServiceListDto>>>, GetAllServicesQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceByIdQuery, Result<ServiceDto?>>, GetServiceByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetServicesByCategoryQuery, Result<IReadOnlyList<ServiceListDto>>>, GetServicesByCategoryQueryHandler>();

        // Registra domain event handlers
        services.AddScoped<IEventHandler<ServiceActivatedDomainEvent>, ServiceActivatedDomainEventHandler>();
        services.AddScoped<IEventHandler<ServiceDeactivatedDomainEvent>, ServiceDeactivatedDomainEventHandler>();

        return services;
    }
}
