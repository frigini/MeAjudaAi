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
        
        // Module API
        services.AddScoped<ICatalogsModuleApi, CatalogsModuleApi>();
        
        return services;
    }
}
