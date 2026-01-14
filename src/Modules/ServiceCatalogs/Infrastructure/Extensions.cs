using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
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
        // Configura DbContext
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
                if (isTestEnvironment)
                {
                    // Same test fallback as DapperConnection
                    connectionString = "Host=localhost;Port=5432;Database=meajudaai_test;Username=postgres;Password=test;";
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

        // Registra repositórios
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

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

        return services;
    }
}
