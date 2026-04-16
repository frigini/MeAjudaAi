using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.Rebus;

/// <summary>
/// Implementação do IMessageBus utilizando Rebus (Enterprise Service Bus)
/// </summary>
public class RebusMessageBus(global::Rebus.Bus.IBus bus, ILogger<RebusMessageBus> logger) : IMessageBus
{
    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Rebus: Sending message of type {MessageType} to queue {QueueName}", 
            typeof(TMessage).Name, queueName ?? "default");
        
        return queueName != null 
            ? bus.Advanced.Routing.Send(queueName, message!) 
            : bus.Send(message!);
    }

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Rebus: Publishing event of type {EventType}", typeof(TMessage).Name);
        
        // No Rebus padrão, o tópico é inferido pelo tipo da mensagem.
        return bus.Publish(@event!);
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Rebus: Explicit subscription requested for {MessageType}", typeof(TMessage).Name);
        return bus.Subscribe<TMessage>();
    }
}
