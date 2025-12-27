using System.Text.Json;
using MeAjudaAi.Shared.Messaging.Options;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.RabbitMq;

/// <summary>
/// Implementação do IMessageBus usando RabbitMQ para ambientes de desenvolvimento e testing
/// </summary>
public class RabbitMqMessageBus(
    RabbitMqOptions options,
    ILogger<RabbitMqMessageBus> logger) : IMessageBus
{
    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        var targetQueue = queueName ?? options.DefaultQueueName;

        logger.LogInformation("RabbitMQ: Sending message of type {MessageType} to queue {QueueName}",
            typeof(TMessage).Name, targetQueue);

        // Em desenvolvimento, apenas registramos as mensagens em log
        // A implementação completa do RabbitMQ seria conectada aqui via Rebus ou RabbitMQ.Client
        logger.LogDebug("RabbitMQ Message Content: {MessageContent}",
            JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true }));

        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        var targetTopic = topicName ?? options.DefaultQueueName;

        logger.LogInformation("RabbitMQ: Publishing event of type {EventType} to topic {TopicName}",
            typeof(TMessage).Name, targetTopic);

        // Em desenvolvimento, apenas registramos os eventos em log
        // A implementação completa do RabbitMQ seria conectada aqui via Rebus ou RabbitMQ.Client
        logger.LogDebug("RabbitMQ Event Content: {EventContent}",
            JsonSerializer.Serialize(@event, new JsonSerializerOptions { WriteIndented = true }));

        return Task.CompletedTask;
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        var subscription = subscriptionName ?? $"{typeof(TMessage).Name}-subscription";

        logger.LogInformation("RabbitMQ: Subscribing to messages of type {MessageType} with subscription {SubscriptionName}",
            typeof(TMessage).Name, subscription);

        // Em desenvolvimento, apenas logamos as subscrições
        // A implementação completa do RabbitMQ seria conectada aqui via Rebus
        return Task.CompletedTask;
    }
}
