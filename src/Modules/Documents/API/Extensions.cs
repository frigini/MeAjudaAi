using MeAjudaAi.Modules.Documents.API.Endpoints;
using MeAjudaAi.Modules.Documents.Application;
using MeAjudaAi.Modules.Documents.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.API;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo Documents.
    /// </summary>
    public static IServiceCollection AddDocumentsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo Documents.
    /// </summary>
    public static WebApplication UseDocumentsModule(this WebApplication app)
    {
        // Garantir que as migrações estão aplicadas
        EnsureDatabaseMigrations(app);

        app.MapDocumentsEndpoints();

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
            var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.DocumentsDbContext>();
            if (context == null) return;

            // Em ambiente de teste E2E, pular migrações automáticas - elas são gerenciadas pelo TestContainer
            if (app.Environment.IsEnvironment("Test") || app.Environment.IsEnvironment("Testing"))
            {
                return;
            }

            // Em produção, usar migrações normais
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Em caso de erro, log mas não quebra a aplicação
            try
            {
                using var scope = app.Services.CreateScope();
                var logger = scope.ServiceProvider.GetService<ILogger<Infrastructure.Persistence.DocumentsDbContext>>();
                logger?.LogWarning(ex, "Falha ao aplicar migrações do módulo Documents. Usando EnsureCreated como fallback.");

                var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.DocumentsDbContext>();
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
