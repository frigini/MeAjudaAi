using MeAjudaAi.Modules.Users.API.Endpoints;
using MeAjudaAi.Modules.Users.Application;
using MeAjudaAi.Modules.Users.Infrastructure;
using MeAjudaAi.Shared.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        if (configuration.GetValue<bool>("Database:EnableSchemaIsolation", false))
        {
            await services.EnsureUsersSchemaPermissionsAsync(configuration, usersRolePassword, appRolePassword);
        }

        return services;
    }

    public static WebApplication UseUsersModule(this WebApplication app)
    {
        app.MapUsersEndpoints();

        return app;
    }
}