using System.Diagnostics;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Strategy;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.ServiceBus;

public class ServiceBusMessageBus : IMessageBus, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly Dictionary<string, ServiceBusSender> _senders = [];
    private readonly Dictionary<string, ServiceBusProcessor> _processors = [];
    private readonly MessageBusOptions _options;
    private readonly ITopicStrategySelector _topicStrategySelector;
    private readonly ILogger<ServiceBusMessageBus> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ServiceBusMessageBus(
        ServiceBusClient client,
        ITopicStrategySelector topicStrategySelector,
        MessageBusOptions options,
        ILogger<ServiceBusMessageBus> logger)
    {
        _client = client;
        _topicStrategySelector = topicStrategySelector;
        _options = options;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task SendAsync<T>(T message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        queueName ??= GetQueueName<T>();
        var sender = GetOrCreateSender(queueName);

        var serviceBusMessage = CreateServiceBusMessage(message);

        try
        {
            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
            _logger.LogDebug("Message {MessageType} sent to queue {QueueName} with MessageId {MessageId}",
                typeof(T).Name, queueName, serviceBusMessage.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message {MessageType} to queue {QueueName}",
                typeof(T).Name, queueName);
            throw;
        }
    }

    public async Task PublishAsync<T>(T @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        topicName ??= GetTopicName<T>();
        var sender = GetOrCreateSender(topicName);

        var serviceBusMessage = CreateServiceBusMessage(@event);

        try
        {
            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
            _logger.LogDebug("Event {EventType} published to topic {TopicName} with MessageId {MessageId}",
                typeof(T).Name, topicName, serviceBusMessage.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to topic {TopicName}",
                typeof(T).Name, topicName);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(
        Func<T, CancellationToken, Task> handler,
        string? subscriptionName = null,
        CancellationToken cancellationToken = default)
    {
        var topicName = GetTopicName<T>();
        subscriptionName ??= GetSubscriptionName<T>();
        var processorKey = $"{topicName}/{subscriptionName}";

        if (_processors.ContainsKey(processorKey))
        {
            _logger.LogWarning("Processor for {ProcessorKey} already exists", processorKey);
            return;
        }

        var processor = _client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = _options.MaxConcurrentCalls,
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            MaxAutoLockRenewalDuration = _options.LockDuration
        });

        processor.ProcessMessageAsync += async args =>
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var message = JsonSerializer.Deserialize<T>(args.Message.Body.ToString(), _jsonOptions);
                // Only validate nullability for reference types; value types are always valid post-deserialization
                if (message is not null || typeof(T).IsValueType)
                {
                    await handler(message!, args.CancellationToken);
                    await args.CompleteMessageAsync(args.Message, args.CancellationToken);

                    _logger.LogDebug("Message {MessageType} processed successfully in {ElapsedMs}ms",
                        typeof(T).Name, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize message {MessageId} as {MessageType}",
                        args.Message.MessageId, typeof(T).Name);
                    await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed",
                        "Could not deserialize message", args.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageType} with MessageId {MessageId}",
                    typeof(T).Name, args.Message.MessageId);

                if (args.Message.DeliveryCount >= _options.MaxDeliveryCount)
                {
                    await args.DeadLetterMessageAsync(args.Message, "MaxDeliveryCountExceeded",
                        ex.Message, args.CancellationToken);
                }
                else
                {
                    await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
                }
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Error in message processor for {EntityPath}", args.EntityPath);
            return Task.CompletedTask;
        };

        _processors[processorKey] = processor;
        await processor.StartProcessingAsync(cancellationToken);

        _logger.LogInformation("Started processing messages for {ProcessorKey}", processorKey);
    }

    private ServiceBusMessage CreateServiceBusMessage<T>(T message)
    {
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var serviceBusMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = typeof(T).Name,
            MessageId = UuidGenerator.NewIdString(),
            TimeToLive = _options.DefaultTimeToLive
        };

        // Adicionar propriedades para filtros
        serviceBusMessage.ApplicationProperties["MessageType"] = typeof(T).Name;

        if (message is IIntegrationEvent integrationEvent)
        {
            serviceBusMessage.ApplicationProperties["Source"] = integrationEvent.Source;
            serviceBusMessage.ApplicationProperties["EventId"] = integrationEvent.Id;
            serviceBusMessage.ApplicationProperties["EventType"] = integrationEvent.EventType;
            serviceBusMessage.ApplicationProperties["OccurredAt"] = integrationEvent.OccurredAt;
        }

        return serviceBusMessage;
    }

    private ServiceBusSender GetOrCreateSender(string entityName)
    {
        if (!_senders.TryGetValue(entityName, out var sender))
        {
            sender = _client.CreateSender(entityName);
            _senders[entityName] = sender;
        }
        return sender;
    }

    private string GetQueueName<T>() => _options.QueueNamingConvention(typeof(T));
    private string GetSubscriptionName<T>() => _options.SubscriptionNamingConvention(typeof(T));
    private string GetTopicName<T>() => _topicStrategySelector.SelectTopicForEvent<T>();

    public async ValueTask DisposeAsync()
    {
        foreach (var processor in _processors.Values)
        {
            await processor.StopProcessingAsync();
            await processor.DisposeAsync();
        }

        foreach (var sender in _senders.Values)
            await sender.DisposeAsync();

        await _client.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
