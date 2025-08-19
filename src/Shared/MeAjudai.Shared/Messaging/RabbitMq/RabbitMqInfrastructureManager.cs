using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;
using MeAjudaAi.Shared.Messaging.Strategy;

namespace MeAjudaAi.Shared.Messaging.RabbitMq;

public interface IRabbitMqInfrastructureManager
{
    Task EnsureInfrastructureAsync();
    Task CreateQueueAsync(string queueName, bool durable = true);
    Task CreateExchangeAsync(string exchangeName, string exchangeType = ExchangeType.Topic);
    Task BindQueueToExchangeAsync(string queueName, string exchangeName, string routingKey = "");
}

public class RabbitMqInfrastructureManager : IRabbitMqInfrastructureManager, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly IEventTypeRegistry _eventRegistry;
    private readonly ITopicStrategySelector _topicSelector;
    private readonly ILogger<RabbitMqInfrastructureManager> _logger;

    public RabbitMqInfrastructureManager(
        IOptions<RabbitMqOptions> options,
        IEventTypeRegistry eventRegistry,
        ITopicStrategySelector topicSelector,
        ILogger<RabbitMqInfrastructureManager> logger)
    {
        _options = options.Value;
        _eventRegistry = eventRegistry;
        _topicSelector = topicSelector;
        _logger = logger;
    }

    public async Task EnsureInfrastructureAsync()
    {
        try
        {
            _logger.LogInformation("Creating RabbitMQ infrastructure...");

            // Create default queue
            await CreateQueueAsync(_options.DefaultQueueName);

            // Create domain-specific queues
            foreach (var domainQueue in _options.DomainQueues)
            {
                await CreateQueueAsync(domainQueue.Value);
            }

            // Create exchanges and bindings for event types
            var eventTypes = await _eventRegistry.GetAllEventTypesAsync();
            foreach (var eventType in eventTypes)
            {
                var queueName = _topicSelector.SelectTopicForEvent(eventType);
                var exchangeName = $"{queueName}.exchange";

                await CreateExchangeAsync(exchangeName, ExchangeType.Topic);
                await CreateQueueAsync(queueName);
                await BindQueueToExchangeAsync(queueName, exchangeName, eventType.Name);

                _logger.LogDebug("Created infrastructure for event type {EventType}: exchange={Exchange}, queue={Queue}", 
                    eventType.Name, exchangeName, queueName);
            }

            _logger.LogInformation("RabbitMQ infrastructure created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ infrastructure");
            throw;
        }
    }

    public Task CreateQueueAsync(string queueName, bool durable = true)
    {
        // TODO: Implement RabbitMQ 7.x async API when RabbitMQ is available
        _logger.LogDebug("Queue creation requested: {QueueName} (durable: {Durable})", queueName, durable);
        return Task.CompletedTask;
    }

    public Task CreateExchangeAsync(string exchangeName, string exchangeType = ExchangeType.Topic)
    {
        // TODO: Implement RabbitMQ 7.x async API when RabbitMQ is available
        _logger.LogDebug("Exchange creation requested: {ExchangeName} (type: {ExchangeType})", exchangeName, exchangeType);
        return Task.CompletedTask;
    }

    public Task BindQueueToExchangeAsync(string queueName, string exchangeName, string routingKey = "")
    {
        // TODO: Implement RabbitMQ 7.x async API when RabbitMQ is available
        _logger.LogDebug("Queue binding requested: {QueueName} to {ExchangeName} with key '{RoutingKey}'", 
            queueName, exchangeName, routingKey);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        // TODO: Implement proper disposal when RabbitMQ connection is implemented
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
