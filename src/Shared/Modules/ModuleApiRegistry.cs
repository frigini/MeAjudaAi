using System.Reflection;
using MeAjudaAi.Shared.Contracts.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Modules;

/// <summary>
/// Serviço para descoberta e registro automático de Module APIs
/// </summary>
public static class ModuleApiRegistry
{
    /// <summary>
    /// Registra automaticamente todas as Module APIs encontradas no assembly
    /// </summary>
    public static IServiceCollection AddModuleApis(this IServiceCollection services, params Assembly[] assemblies)
    {
        var moduleTypes = new List<Type>();

        // Se nenhum assembly for especificado, usa o assembly atual
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i => i == typeof(IModuleApi)))
                .Where(t => t.GetCustomAttribute<ModuleApiAttribute>() != null);

            moduleTypes.AddRange(types);
        }

        foreach (var moduleType in moduleTypes)
        {
            var interfaces = moduleType.GetInterfaces()
                .Where(i => i != typeof(IModuleApi) && typeof(IModuleApi).IsAssignableFrom(i));

            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, moduleType);
            }

            services.AddScoped(typeof(IModuleApi), moduleType);
        }

        return services;
    }

    /// <summary>
    /// Obtém informações sobre todas as Module APIs registradas
    /// </summary>
    public static async Task<IReadOnlyList<ModuleApiInfo>> GetRegisteredModulesAsync(IServiceProvider serviceProvider)
    {
        var moduleApis = serviceProvider.GetServices<IModuleApi>();
        var moduleInfos = new List<ModuleApiInfo>();

        foreach (var api in moduleApis)
        {
            var isAvailable = await api.IsAvailableAsync();
            moduleInfos.Add(new ModuleApiInfo(
                api.ModuleName,
                api.ApiVersion,
                api.GetType().FullName!,
                isAvailable
            ));
        }

        return moduleInfos;
    }
}
