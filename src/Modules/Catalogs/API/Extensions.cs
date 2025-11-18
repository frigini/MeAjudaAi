using MeAjudaAi.Modules.Catalogs.API.Endpoints;
using MeAjudaAi.Modules.Catalogs.Application;
using MeAjudaAi.Modules.Catalogs.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Catalogs.API;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo Catalogs.
    /// </summary>
    public static IServiceCollection AddCatalogsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddCatalogsInfrastructure(configuration);

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo Catalogs.
    /// </summary>
    public static WebApplication UseCatalogsModule(this WebApplication app)
    {
        // Garantir que as migrações estão aplicadas
        EnsureDatabaseMigrations(app);

        app.MapCatalogsEndpoints();

        return app;
    }

    private static void EnsureDatabaseMigrations(WebApplication app)
    {
        if (app?.Services == null) return;

        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.CatalogsDbContext>();
            if (context == null) return;

            // Em ambiente de teste, pular migrações automáticas
            if (app.Environment.IsEnvironment("Test") || app.Environment.IsEnvironment("Testing"))
            {
                return;
            }

            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var logger = scope.ServiceProvider.GetService<ILogger<Infrastructure.Persistence.CatalogsDbContext>>();
                logger?.LogWarning(ex, "Falha ao aplicar migrações do módulo Catalogs. Usando EnsureCreated como fallback.");

                var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.CatalogsDbContext>();
                if (context != null)
                {
                    context.Database.EnsureCreated();
                }
            }
            catch
            {
                // Se ainda falhar, ignora silenciosamente
            }
        }
    }
}
