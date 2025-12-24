using FluentValidation;
using MeAjudaAi.Shared.Behaviors;
using MeAjudaAi.Shared.Mediator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MeAjudaAi.Shared.Extensions;

public static class Extensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        // Escanear assemblies explicitamente para garantir que todos os validators sejam registrados
        var assembliesToScan = new List<Assembly>();
        
        var assemblyNames = new[]
        {
            "MeAjudaAi.Shared",
            "MeAjudaAi.Modules.Users.Application",
            "MeAjudaAi.Modules.Providers.Application",
            "MeAjudaAi.Modules.Documents.Application",
            "MeAjudaAi.Modules.Locations.Application",
            "MeAjudaAi.Modules.SearchProviders.Application",
            "MeAjudaAi.Modules.ServiceCatalogs.Application"
        };

        foreach (var assemblyName in assemblyNames)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                assembliesToScan.Add(assembly);
            }
            catch
            {
                // Assembly não encontrado - ignorar silenciosamente
                // Isso pode acontecer em ambientes de teste parcial
            }
        }

        services.AddValidatorsFromAssemblies(assembliesToScan);

        // Registra behaviors do pipeline CQRS (ordem importa!)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        return services;
    }
}
