using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Events;

/// <summary>
/// Extension methods para configuração de Events (Domain Events)
/// </summary>
public static class EventsExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection services)
    {
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        services.AddScoped<IDomainEventProcessor, DomainEventProcessor>();

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(IEvent))
            .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
