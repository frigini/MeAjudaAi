using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.NoOp;

/// <summary>
/// Implementação do IMessageBus que não faz nada - para uso em testes ou quando messaging está desabilitado
/// </summary>
public class NoOpMessageBus : IMessageBus
{
    private readonly ILogger<NoOpMessageBus> _logger;

    public NoOpMessageBus(ILogger<NoOpMessageBus> logger)
    {
        _logger = logger;
    }

    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NoOpMessageBus: Ignoring message of type {MessageType} to queue {QueueName}", 
            typeof(TMessage).Name, queueName ?? "default");
        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NoOpMessageBus: Ignoring event of type {EventType} to topic {TopicName}", 
            typeof(TMessage).Name, topicName ?? "default");
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NoOpMessageBus: Ignoring subscription to messages of type {MessageType} with subscription {SubscriptionName}", 
            typeof(TMessage).Name, subscriptionName ?? "default");
        return Task.CompletedTask;
    }
}