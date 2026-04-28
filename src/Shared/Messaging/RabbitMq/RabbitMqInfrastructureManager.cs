using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Messaging.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MeAjudaAi.Shared.Messaging.RabbitMq;

internal class RabbitMqInfrastructureManager : IRabbitMqInfrastructureManager, IAsyncDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly IEventTypeRegistry _eventRegistry;
    private readonly ILogger<RabbitMqInfrastructureManager> _logger;
    private readonly SemaphoreSlim _channelLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqInfrastructureManager(
        IConnectionFactory connectionFactory,
        RabbitMqOptions options,
        IEventTypeRegistry eventRegistry,
        ILogger<RabbitMqInfrastructureManager> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options;
        _eventRegistry = eventRegistry;
        _logger = logger;
    }

    private async Task<IChannel> GetChannelAsync()
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _channelLock.WaitAsync();
        try
        {
            // Verificação dupla após adquirir o lock
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            var oldChannel = _channel;
            
            if (_connection is null || !_connection.IsOpen)
            {
                var oldConnection = _connection;
                _connection = await _connectionFactory.CreateConnectionAsync();
                
                if (oldConnection is not null)
                {
                    await oldConnection.DisposeAsync();
                }
            }

            _channel = await _connection.CreateChannelAsync();

            // Remove o canal antigo não aberto se existir
            if (oldChannel is not null)
            {
                await oldChannel.DisposeAsync();
            }

            return _channel;
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async Task EnsureInfrastructureAsync()
    {
        try
        {
            _logger.LogInformation("Creating RabbitMQ infrastructure...");
            var channel = await GetChannelAsync();

            // Cria fila padrão
            await CreateQueueAsync(_options.DefaultQueueName);

            // Cria filas específicas de domínio
            foreach (var domainQueue in _options.DomainQueues)
            {
                await CreateQueueAsync(domainQueue.Value);
            }

            // Cria exchanges e bindings para tipos de eventos
            var eventTypes = await _eventRegistry.GetAllEventTypesAsync();
            var exchangeName = $"{_options.DefaultQueueName}.exchange";

            await CreateExchangeAsync(exchangeName, ExchangeType.Topic);

            var allQueues = new List<string> { _options.DefaultQueueName };
            foreach (var queue in _options.DomainQueues.Values)
            {
                if (!allQueues.Contains(queue))
                {
                    allQueues.Add(queue);
                }
            }

            var eventNames = eventTypes.Select(e => e.FullName).Distinct();
            foreach (var eventName in eventNames)
            {
                if (string.IsNullOrWhiteSpace(eventName))
                {
                    _logger.LogWarning("Skipping event type with null/empty FullName.");
                    continue;
                }

                foreach (var queueName in allQueues)
                {
                    await BindQueueToExchangeAsync(queueName, exchangeName, eventName);

                    _logger.LogDebug("Infrastructure created for event type {EventType}: exchange={Exchange}, queue={Queue}",
                        eventName, exchangeName, queueName);
                }
            }

            _logger.LogInformation("RabbitMQ infrastructure setup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ infrastructure");
            throw new InvalidOperationException(
                "Failed to create RabbitMQ infrastructure (exchanges, queues, and bindings for registered event types)",
                ex);
        }
    }

    public async Task CreateQueueAsync(string queueName, bool durable = true)
    {
        await _channelLock.WaitAsync();
        try
        {
            var channel = await GetChannelAsync();
            _logger.LogDebug("Declaring queue: {QueueName} (durable: {Durable})", queueName, durable);
            
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: durable,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async Task CreateExchangeAsync(string exchangeName, string exchangeType = ExchangeType.Topic)
    {
        await _channelLock.WaitAsync();
        try
        {
            var channel = await GetChannelAsync();
            _logger.LogDebug("Declaring exchange: {ExchangeName} (type: {ExchangeType})", exchangeName, exchangeType);
            
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: exchangeType,
                durable: true,
                autoDelete: false,
                arguments: null);
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async Task BindQueueToExchangeAsync(string queueName, string exchangeName, string routingKey = "")
    {
        await _channelLock.WaitAsync();
        try
        {
            var channel = await GetChannelAsync();
            _logger.LogDebug("Binding queue {QueueName} to exchange {ExchangeName} with routing key '{RoutingKey}'",
                queueName, exchangeName, routingKey);
            
            await channel.QueueBindAsync(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey,
                arguments: null);
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
        _channelLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
