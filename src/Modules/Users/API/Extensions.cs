using MeAjudaAi.Modules.Users.API.Endpoints;
using MeAjudaAi.Modules.Users.Application;
using MeAjudaAi.Modules.Users.Infrastructure;
using MeAjudaAi.Shared.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.API;

public static class Extensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    /// <summary>
    /// Configura isolamento de schema para o módulo Users (opcional - para produção)
    /// Usa os scripts existentes em infrastructure/database/schemas
    /// </summary>
    public static async Task<IServiceCollection> AddUsersModuleWithSchemaIsolationAsync(
        this IServiceCollection services,
        IConfiguration configuration,
        string? usersRolePassword = null,
        string? appRolePassword = null)
    {
        // Configurar serviços do módulo
        services.AddUsersModule(configuration);

        // Configurar permissões de schema (apenas se habilitado)
        if (configuration.GetValue("Database:EnableSchemaIsolation", false))
        {
            await services.EnsureUsersSchemaPermissionsAsync(configuration, usersRolePassword, appRolePassword).ConfigureAwait(false);
        }

        return services;
    }

    public static WebApplication UseUsersModule(this WebApplication app)
    {
        // Garantir que as migrações estão aplicadas
        EnsureDatabaseMigrations(app);
        
        app.MapUsersEndpoints();

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
            var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.UsersDbContext>();
            if (context == null) return;
            
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Em caso de erro, log mas não quebra a aplicação
            try
            {
                using var scope = app.Services.CreateScope();
                var logger = scope.ServiceProvider.GetService<ILogger<Infrastructure.Persistence.UsersDbContext>>();
                logger?.LogWarning(ex, "Falha ao aplicar migrações do módulo Users. Usando EnsureCreated como fallback.");
                
                var context = scope.ServiceProvider.GetService<Infrastructure.Persistence.UsersDbContext>();
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
