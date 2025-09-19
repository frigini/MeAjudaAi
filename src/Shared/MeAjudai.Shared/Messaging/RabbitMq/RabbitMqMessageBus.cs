using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MeAjudaAi.Shared.Messaging.RabbitMq;

/// <summary>
/// Implementação do IMessageBus usando RabbitMQ para ambientes de desenvolvimento e testing
/// </summary>
public class RabbitMqMessageBus : IMessageBus
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqMessageBus> _logger;

    public RabbitMqMessageBus(
        RabbitMqOptions options,
        ILogger<RabbitMqMessageBus> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        var targetQueue = queueName ?? _options.DefaultQueueName;
        
        _logger.LogInformation("RabbitMQ: Sending message of type {MessageType} to queue {QueueName}", 
            typeof(TMessage).Name, targetQueue);

        // Em desenvolvimento, apenas registramos as mensagens em log
        // A implementação completa do RabbitMQ seria conectada aqui via Rebus ou RabbitMQ.Client
        _logger.LogDebug("RabbitMQ Message Content: {MessageContent}", 
            JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true }));

        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        var targetTopic = topicName ?? _options.DefaultQueueName;
        
        _logger.LogInformation("RabbitMQ: Publishing event of type {EventType} to topic {TopicName}", 
            typeof(TMessage).Name, targetTopic);

        // Em desenvolvimento, apenas registramos os eventos em log
        // A implementação completa do RabbitMQ seria conectada aqui via Rebus ou RabbitMQ.Client
        _logger.LogDebug("RabbitMQ Event Content: {EventContent}", 
            JsonSerializer.Serialize(@event, new JsonSerializerOptions { WriteIndented = true }));

        return Task.CompletedTask;
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        var subscription = subscriptionName ?? $"{typeof(TMessage).Name}-subscription";
        
        _logger.LogInformation("RabbitMQ: Subscribing to messages of type {MessageType} with subscription {SubscriptionName}", 
            typeof(TMessage).Name, subscription);

        // Em desenvolvimento, apenas logamos as subscrições
        // A implementação completa do RabbitMQ seria conectada aqui via Rebus
        return Task.CompletedTask;
    }
}