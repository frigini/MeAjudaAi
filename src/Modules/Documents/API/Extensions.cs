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

        // Register module public API for cross-module communication
        services.AddScoped<IDocumentsModuleApi, DocumentsModuleApi>();

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

        // Em ambiente de teste E2E, pular migrações automáticas - elas são gerenciadas pelo TestContainer
        if (app.Environment.IsEnvironment("Test") || app.Environment.IsEnvironment("Testing"))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.DocumentsDbContext>();
        if (context == null)
        {
            var logger = scope.ServiceProvider.GetService<ILogger<Infrastructure.Persistence.DocumentsDbContext>>();
            logger?.LogWarning("DocumentsDbContext not registered. Skipping migrations.");
            return;
        }

        var contextLogger = scope.ServiceProvider.GetService<ILogger<Infrastructure.Persistence.DocumentsDbContext>>();

        try
        {
            // Em produção, usar migrações normais
            // Nota: Em ambientes com múltiplas instâncias, considere executar migrações
            // via pipeline de deployment ao invés de startup automático
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Log do erro, mas tenta fallback
            contextLogger?.LogWarning(ex, "Falha ao aplicar migrações do módulo Documents. Usando EnsureCreated como fallback.");

            // Tenta EnsureCreated como fallback (apenas em desenvolvimento)
            if (app.Environment.IsDevelopment())
            {
                context.Database.EnsureCreated();
            }
            else
            {
                // Em produção, não fazer fallback silencioso - relançar para visão do problema
                contextLogger?.LogError(ex, "Erro crítico ao aplicar migrações do módulo Documents em ambiente de produção.");
                throw;
            }
        }
    }
}
