using MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints;
using MeAjudaAi.Modules.ServiceCatalogs.Application;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
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
        app.MapServiceCatalogsEndpoints();

        return app;
    }
}
