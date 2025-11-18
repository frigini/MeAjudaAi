using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;
using MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries;
using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Catalogs.Infrastructure;

public static class Extensions
{
    /// <summary>
    /// Adds Catalogs module infrastructure services.
    /// </summary>
    public static IServiceCollection AddCatalogsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure DbContext
        services.AddDbContext<CatalogsDbContext>((serviceProvider, options) =>
        {
            var environment = serviceProvider.GetService<IHostEnvironment>();
            var isTestEnvironment = environment?.EnvironmentName == "Testing";

            // Use shared connection string resolution logic (same precedence as DapperConnection)
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                  ?? configuration.GetConnectionString("Catalogs")
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
                npgsqlOptions.MigrationsAssembly(typeof(CatalogsDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "catalogs");
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);
        });

        // Register repositories
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        // Register command handlers
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

        // Register query handlers
        services.AddScoped<IQueryHandler<GetAllServiceCategoriesQuery, Result<IReadOnlyList<ServiceCategoryDto>>>, GetAllServiceCategoriesQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceCategoryByIdQuery, Result<ServiceCategoryDto?>>, GetServiceCategoryByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceCategoriesWithCountQuery, Result<IReadOnlyList<ServiceCategoryWithCountDto>>>, GetServiceCategoriesWithCountQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllServicesQuery, Result<IReadOnlyList<ServiceListDto>>>, GetAllServicesQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceByIdQuery, Result<ServiceDto?>>, GetServiceByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetServicesByCategoryQuery, Result<IReadOnlyList<ServiceListDto>>>, GetServicesByCategoryQueryHandler>();

        return services;
    }
}
