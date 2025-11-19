using MeAjudaAi.Modules.ServiceCatalogs.Application.ModuleApi;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Note: Handlers are automatically registered through reflection in Infrastructure layer
        // via AddApplicationHandlers() which scans the Application assembly

        // Module API - register both interface and concrete type for DI flexibility
        services.AddScoped<IServiceCatalogsModuleApi, ServiceCatalogsModuleApi>();
        services.AddScoped<ServiceCatalogsModuleApi>();

        return services;
    }
}
