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
        
        // Apenas assemblies que contêm FluentValidation validators
        var assemblyNames = new[]
        {
            "MeAjudaAi.Modules.Users.Application",
            "MeAjudaAi.Modules.Providers.Application",
            "MeAjudaAi.Modules.SearchProviders.Application"
        };

        // Em cenários de teste parcial, alguns assemblies podem não estar disponíveis
        foreach (var assemblyName in assemblyNames)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                assembliesToScan.Add(assembly);
            }
            catch (FileNotFoundException)
            {
                // Expected in partial-test scenarios - assembly not loaded
                Console.WriteLine($"Validator assembly not found: {assemblyName}");
            }
            catch (BadImageFormatException ex)
            {
                // Expected for platform-specific assemblies
                Console.WriteLine($"Invalid assembly format: {assemblyName} - {ex.Message}");
            }
            // Any other exception (security, permissions, etc.) should bubble up
        }

        services.AddValidatorsFromAssemblies(assembliesToScan);

        // Registra behaviors do pipeline CQRS (ordem importa!)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        return services;
    }
}
