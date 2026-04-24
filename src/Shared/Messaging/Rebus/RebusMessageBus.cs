using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.Rebus;

/// <summary>
/// Implementação do IMessageBus utilizando Rebus (Enterprise Service Bus)
/// </summary>
[ExcludeFromCodeCoverage]
public class RebusMessageBus(global::Rebus.Bus.IBus bus, ILogger<RebusMessageBus> logger) : IMessageBus
{
    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var targetQueue = queueName ?? "default";
        logger.LogDebug("Rebus: Sending message of type {MessageType} to queue {QueueName}", 
            typeof(TMessage).Name, targetQueue);
        
        return !string.IsNullOrWhiteSpace(queueName) 
            ? bus.Advanced.Routing.Send(queueName, message!) 
            : bus.Send(message!);
    }

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!string.IsNullOrWhiteSpace(topicName))
        {
            logger.LogWarning("Rebus: Manual topic name {TopicName} provided for event {EventType}, but Rebus implementation defaults to Type-based routing. The topic name will be ignored.", 
                topicName, typeof(TMessage).Name);
        }

        logger.LogDebug("Rebus: Publishing event of type {EventType}", typeof(TMessage).Name);
        
        // No Rebus padrão, o tópico é inferido pelo tipo da mensagem.
        return bus.Publish(@event!);
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task>? handler = null, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        if (handler != null)
        {
            logger.LogWarning("Rebus: Explicit handler provided to SubscribeAsync<{MessageType}> will be ignored. Callers must register IHandleMessages<{MessageType}> via Dependency Injection.", 
                typeof(TMessage).Name, typeof(TMessage).Name);
            
            throw new NotSupportedException($"Explicit delegate handlers are not supported in RebusMessageBus. Please register a class implementing IHandleMessages<{typeof(TMessage).Name}> in the DI container.");
        }

        logger.LogInformation("Rebus: Explicit subscription requested for {MessageType}", typeof(TMessage).Name);
        return bus.Subscribe<TMessage>();
    }
}
