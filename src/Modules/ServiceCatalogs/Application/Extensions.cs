using MeAjudaAi.Modules.ServiceCatalogs.Application.ModuleApi;
// TODO Phase 2: Uncomment when shared contracts are added
// using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Note: Handlers are automatically registered through reflection in Infrastructure layer
        // via AddApplicationHandlers() which scans the Application assembly

        // Module API - register concrete type for DI (interface registration in Phase 2)
        // TODO Phase 2: Uncomment interface registration when IServiceCatalogsModuleApi is added
        // services.AddScoped<IServiceCatalogsModuleApi, ServiceCatalogsModuleApi>();
        services.AddScoped<ServiceCatalogsModuleApi>();

        return services;
    }
}
