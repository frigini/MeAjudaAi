using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints;
using MeAjudaAi.Modules.ServiceCatalogs.Application;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.ServiceCatalogs.API;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo ServiceCatalogs.
    /// </summary>
    public static IServiceCollection AddServiceCatalogsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        // Registrar API pública do módulo para comunicação entre módulos
        services.AddScoped<IServiceCatalogsModuleApi, Application.ModuleApi.ServiceCatalogsModuleApi>();

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo ServiceCatalogs.
    /// </summary>
    public static WebApplication UseServiceCatalogsModule(this WebApplication app)
    {
        app.MapServiceCatalogsEndpoints();

        return app;
    }
}
