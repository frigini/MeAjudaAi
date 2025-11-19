using MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints;
using MeAjudaAi.Modules.ServiceCatalogs.Application;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure;
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
                logger?.LogWarning(ex, "Falha ao aplicar migrações do módulo ServiceCatalogs. Usando EnsureCreated como fallback em Development.");
                try
                {
                    var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.ServiceCatalogsDbContext>();
                    context?.Database.EnsureCreated();
                }
                catch (Exception fallbackEx)
                {
                    logger?.LogError(fallbackEx, "Falha crítica ao inicializar o banco do módulo ServiceCatalogs.");
                    throw; // Fail fast even in Development if EnsureCreated fails
                }
            }
            else
            {
                // Fail fast in non-development environments
                logger?.LogError(ex, "Falha crítica ao aplicar migrações do módulo ServiceCatalogs em ambiente de produção.");
                throw;
            }
        }
    }
}
