using FluentValidation;
using MeAjudaAi.Shared.Behaviors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Common;

public static class Extensions
{
    // Removido AddStructuredLogging daqui - usar o do Logging/SerilogConfigurator.cs

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        // Configurar FluentValidation para assemblies da aplicação
        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("MeAjudaAi") == true)
            .ToArray());

        // Registra behaviors do pipeline CQRS (ordem importa!)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        return services;
    }
}