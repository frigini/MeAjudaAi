using MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints;
using MeAjudaAi.Modules.ServiceCatalogs.Application;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.API;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo ServiceCatalogs.
    /// </summary>
    public static IServiceCollection AddServiceCatalogsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddServiceCatalogsInfrastructure(configuration);

        // Register module public API for cross-module communication
        services.AddScoped<IServiceCatalogsModuleApi, Application.ModuleApi.ServiceCatalogsModuleApi>();

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo ServiceCatalogs.
    /// </summary>
    public static WebApplication UseServiceCatalogsModule(this WebApplication app)
    {
        // Garantir que as migrações estão aplicadas
        EnsureDatabaseMigrations(app);

        app.MapServiceCatalogsEndpoints();

        return app;
    }

    private static void EnsureDatabaseMigrations(WebApplication app)
    {
        if (app?.Services == null) return;

        try
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetService<ILogger<Infrastructure.Persistence.ServiceCatalogsDbContext>>();
            var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.ServiceCatalogsDbContext>();

            if (context == null)
            {
                logger?.LogWarning("ServiceCatalogsDbContext not found in DI container. Skipping migrations.");
                return;
            }

            // Em ambiente de teste, pular migrações automáticas
            if (app.Environment.IsEnvironment("Test") || app.Environment.IsEnvironment("Testing"))
            {
                logger?.LogInformation("Skipping ServiceCatalogs migrations in test environment: {Environment}", app.Environment.EnvironmentName);
                return;
            }

            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetService<ILogger<Infrastructure.Persistence.ServiceCatalogsDbContext>>();

            // Only fallback to EnsureCreated in Development
            if (app.Environment.IsDevelopment())
            {
                logger?.LogWarning(ex, "Failed to apply migrations for ServiceCatalogs module. Using EnsureCreated as fallback in Development.");
                try
                {
                    var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.ServiceCatalogsDbContext>();
                    context?.Database.EnsureCreated();
                }
                catch (Exception fallbackEx)
                {
                    logger?.LogError(fallbackEx, "Critical failure initializing ServiceCatalogs module database.");
                    throw new InvalidOperationException("Falha crítica ao inicializar o banco de dados do módulo ServiceCatalogs após tentativa de fallback.", fallbackEx);
                }
            }
            else
            {
                // Fail fast in non-development environments
                logger?.LogError(ex, "Critical failure applying migrations for ServiceCatalogs module in production environment.");
                throw new InvalidOperationException("Falha ao aplicar migrações do módulo ServiceCatalogs em ambiente de produção. Verifique a conexão com o banco de dados.", ex);
            }
        }
    }
}
