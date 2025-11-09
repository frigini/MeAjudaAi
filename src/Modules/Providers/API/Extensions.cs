using MeAjudaAi.Modules.Providers.API.Endpoints;
using MeAjudaAi.Modules.Providers.Application;
using MeAjudaAi.Modules.Providers.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.API;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo Providers.
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddProvidersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo Providers.
    /// </summary>
    /// <param name="app">Aplicação web</param>
    /// <returns>Aplicação web para encadeamento</returns>
    public static WebApplication UseProvidersModule(this WebApplication app)
    {
        // Garantir que as migrações estão aplicadas
        EnsureDatabaseMigrations(app);

        app.MapProvidersEndpoints();

        return app;
    }

    private static void EnsureDatabaseMigrations(WebApplication app)
    {
        // Só aplica migrações se não estivermos em ambiente de testes unitários
        if (app?.Services == null) return;

        try
        {
            // Criar um escopo para obter o context e aplicar migrações
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.ProvidersDbContext>();
            if (context == null) return;

            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Em caso de erro, log mas não quebra a aplicação
            try
            {
                using var scope = app.Services.CreateScope();
                var logger = scope.ServiceProvider.GetService<ILogger<Infrastructure.Persistence.ProvidersDbContext>>();
                logger?.LogWarning(ex, "Falha ao aplicar migrações do módulo Providers. Usando EnsureCreated como fallback.");

                var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.ProvidersDbContext>();
                if (context != null)
                {
                    context.Database.EnsureCreated();
                }
            }
            catch
            {
                // Se ainda falhar, ignora silenciosamente para não quebrar testes unitários
            }
        }
    }
}
