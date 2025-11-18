using MeAjudaAi.Modules.Catalogs.Application.ModuleApi;
using MeAjudaAi.Shared.Contracts.Modules.Catalogs;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Catalogs.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Note: Handlers are automatically registered through reflection in Infrastructure layer
        // via AddApplicationHandlers() which scans the Application assembly

        // Module API - register both interface and concrete type for DI flexibility
        services.AddScoped<ICatalogsModuleApi, CatalogsModuleApi>();
        services.AddScoped<CatalogsModuleApi>();

        return services;
    }
}
