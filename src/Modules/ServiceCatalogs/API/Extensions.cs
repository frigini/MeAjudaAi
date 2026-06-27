using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints;
using MeAjudaAi.Modules.ServiceCatalogs.Application;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddServiceCatalogsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);
        services.AddScoped<IServiceCatalogsModuleApi, Application.ModuleApi.ServiceCatalogsModuleApi>();

        return services;
    }

    public static IEndpointRouteBuilder UseServiceCatalogsModule(this IEndpointRouteBuilder app)
    {
        ServiceCatalogsModuleEndpoints.Map(app);
        return app;
    }
}