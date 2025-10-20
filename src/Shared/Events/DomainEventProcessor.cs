using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Events;

public class DomainEventProcessor(IServiceProvider serviceProvider) : IDomainEventProcessor
{
    public async Task ProcessDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await ProcessSingleEventAsync(domainEvent, cancellationToken);
        }
    }

    private async Task ProcessSingleEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var eventType = domainEvent.GetType();

        // Buscar todos os handlers para este tipo de evento
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType);
        var handlersList = handlers.ToList();

        // Executar todos os handlers
        foreach (var handler in handlersList)
        {
            var method = handlerType.GetMethod(nameof(IEventHandler<IDomainEvent>.HandleAsync));
            if (method != null && handler != null)
            {
                var task = (Task)method.Invoke(handler, [domainEvent, cancellationToken])!;
                await task;
            }
        }
    }
}
