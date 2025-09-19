using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Events;

public class DomainEventProcessor : IDomainEventProcessor
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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
        var handlers = _serviceProvider.GetServices(handlerType);
        var handlersList = handlers.ToList();

        // Executar todos os handlers
        foreach (var handler in handlersList)
        {
            var method = handlerType.GetMethod(nameof(IEventHandler<IDomainEvent>.HandleAsync));
            if (method != null && handler != null)
            {
                var task = (Task)method.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
                await task;
            }
        }
    }
}