using FluentValidation;
using MeAjudaAi.Shared.Behaviors;
using MeAjudaAi.Shared.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Extensions;

public static class Extensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        // Configurar FluentValidation para assemblies da aplicação
        services.AddValidatorsFromAssemblies([.. AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName?.Contains("MeAjudaAi") == true)]);

        // Registra behaviors do pipeline CQRS (ordem importa!)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        return services;
    }
}