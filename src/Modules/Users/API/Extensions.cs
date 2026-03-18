using FluentValidation;
using MeAjudaAi.Modules.Users.API.Endpoints;
using MeAjudaAi.Modules.Users.Application;
using MeAjudaAi.Modules.Users.Infrastructure;
using MeAjudaAi.Shared.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.API;

public static class Extensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddApplication();
        services.AddInfrastructure(configuration);
        
        // Registrar validadores FluentValidation do módulo Users
        services.AddValidatorsFromAssemblyContaining<Application.Validators.CreateUserRequestValidator>();

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
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

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
        app.MapUsersEndpoints();

        return app;
    }
}
