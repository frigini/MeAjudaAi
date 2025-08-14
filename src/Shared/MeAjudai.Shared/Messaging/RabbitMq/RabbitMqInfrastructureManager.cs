using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;

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
    private readonly IConnection _connection;
    private readonly IModel _channel;

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

        var factory = new ConnectionFactory();
        if (Uri.TryCreate(_options.ConnectionString, UriKind.Absolute, out var uri))
        {
            factory.Uri = uri;
        }
        else
        {
            factory.HostName = _options.Host;
            factory.Port = _options.Port;
            factory.UserName = _options.Username;
            factory.Password = _options.Password;
            factory.VirtualHost = _options.VirtualHost;
        }

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
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
        try
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: durable,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    // Add dead letter exchange for failed messages
                    ["x-dead-letter-exchange"] = $"{queueName}.dlx",
                    ["x-dead-letter-routing-key"] = "failed"
                });

            // Create dead letter queue
            _channel.QueueDeclare(
                queue: $"{queueName}.dlq",
                durable: durable,
                exclusive: false,
                autoDelete: false);

            // Create dead letter exchange
            _channel.ExchangeDeclare($"{queueName}.dlx", ExchangeType.Direct, durable);
            _channel.QueueBind($"{queueName}.dlq", $"{queueName}.dlx", "failed");

            _logger.LogDebug("Created queue: {QueueName}", queueName);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create queue {QueueName}", queueName);
            throw;
        }
    }

    public Task CreateExchangeAsync(string exchangeName, string exchangeType = ExchangeType.Topic)
    {
        try
        {
            _channel.ExchangeDeclare(exchangeName, exchangeType, durable: true);
            _logger.LogDebug("Created exchange: {ExchangeName} of type {ExchangeType}", exchangeName, exchangeType);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create exchange {ExchangeName}", exchangeName);
            throw;
        }
    }

    public Task BindQueueToExchangeAsync(string queueName, string exchangeName, string routingKey = "")
    {
        try
        {
            _channel.QueueBind(queueName, exchangeName, routingKey);
            _logger.LogDebug("Bound queue {QueueName} to exchange {ExchangeName} with routing key '{RoutingKey}'", 
                queueName, exchangeName, routingKey);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bind queue {QueueName} to exchange {ExchangeName}", queueName, exchangeName);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
