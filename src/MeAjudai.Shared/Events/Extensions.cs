using Microsoft.Extensions.DependencyInjection;

namespace MeAjudai.Shared.Events;

internal static class Extensions
{
    public static IServiceCollection AddEvents(this IServiceCollection services)
    {
        services.AddSingleton<IEventDispatcher, EventDispatcher>();

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(IEvent))
            .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}