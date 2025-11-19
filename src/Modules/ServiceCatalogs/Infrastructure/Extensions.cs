using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Database;
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
        // Configure DbContext
        services.AddDbContext<ServiceCatalogsDbContext>((serviceProvider, options) =>
        {
            var environment = serviceProvider.GetService<IHostEnvironment>();
            var isTestEnvironment = environment?.EnvironmentName == "Testing";

            // Use shared connection string resolution logic (same precedence as DapperConnection)
            // Try PostgresOptions first (bound from Postgres:ConnectionString)
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
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "ServiceCatalogs");
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);
        });

        // Register repositories
        // Register repositories
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        // Note: Command and Query handlers will be registered in Phase 1b when Application layer is added

        return services;
    }
}
