using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Events;

public sealed class EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> logger) : IEventDispatcher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var handlers = serviceProvider.GetServices<IEventHandler<TEvent>>();

        var tasks = handlers.Select(handler =>
            HandleSafely(handler, @event, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var tasks = events.Select(@event => PublishAsync(@event, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task HandleSafely<TEvent>(IEventHandler<TEvent> handler, TEvent @event, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        try
        {
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling event {EventType} with handler {HandlerType}",
                @event.EventType, handler.GetType().Name);
        }
    }
}