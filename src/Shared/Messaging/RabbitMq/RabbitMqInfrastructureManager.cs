using MeAjudaAi.Shared.Messaging.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MeAjudaAi.Shared.Messaging.RabbitMq;

internal class RabbitMqInfrastructureManager(
    IConnectionFactory connectionFactory,
    RabbitMqOptions options,
    IEventTypeRegistry eventRegistry,
    ILogger<RabbitMqInfrastructureManager> logger) : IRabbitMqInfrastructureManager, IAsyncDisposable
{
    private readonly SemaphoreSlim _channelLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;

    private async Task<IChannel> GetChannelAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(RabbitMqInfrastructureManager));

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
                _connection = await connectionFactory.CreateConnectionAsync();
                
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
            logger.LogInformation("Creating RabbitMQ infrastructure...");
            var channel = await GetChannelAsync();

            // Cria fila padrão
            await CreateQueueAsync(options.DefaultQueueName);

            // Cria filas específicas de domínio
            foreach (var domainQueue in options.DomainQueues)
            {
                await CreateQueueAsync(domainQueue.Value);
            }

            // Cria exchanges e bindings para tipos de eventos
            var eventTypes = await eventRegistry.GetAllEventTypesAsync();
            var exchangeName = $"{options.DefaultQueueName}.exchange";

            await CreateExchangeAsync(exchangeName, ExchangeType.Topic);

            var allQueues = new List<string> { options.DefaultQueueName };
            foreach (var queue in options.DomainQueues.Values)
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
                    logger.LogWarning("Skipping event type with null/empty FullName.");
                    continue;
                }

                foreach (var queueName in allQueues)
                {
                    await BindQueueToExchangeAsync(queueName, exchangeName, eventName);

                    logger.LogDebug("Infrastructure created for event type {EventType}: exchange={Exchange}, queue={Queue}",
                        eventName, exchangeName, queueName);
                }
            }

            logger.LogInformation("RabbitMQ infrastructure setup completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create RabbitMQ infrastructure");
            throw new InvalidOperationException(
                "Failed to create RabbitMQ infrastructure (exchanges, queues, and bindings for registered event types)",
                ex);
        }
    }

    public async Task CreateQueueAsync(string queueName, bool durable = true)
    {
        // GetChannelAsync gerencia o lock internamente - não há necessidade de travar aqui
        var channel = await GetChannelAsync();
        logger.LogDebug("Declaring queue: {QueueName} (durable: {Durable})", queueName, durable);
        
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: durable,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public async Task CreateExchangeAsync(string exchangeName, string exchangeType = ExchangeType.Topic)
    {
        // GetChannelAsync gerencia o lock internamente - não há necessidade de travar aqui
        var channel = await GetChannelAsync();
        logger.LogDebug("Declaring exchange: {ExchangeName} (type: {ExchangeType})", exchangeName, exchangeType);
        
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: exchangeType,
            durable: true,
            autoDelete: false,
            arguments: null);
    }

    public async Task BindQueueToExchangeAsync(string queueName, string exchangeName, string routingKey = "")
    {
        // GetChannelAsync gerencia o lock internamente - não há necessidade de travar aqui
        var channel = await GetChannelAsync();
        logger.LogDebug("Binding queue {QueueName} to exchange {ExchangeName} with routing key '{RoutingKey}'",
            queueName, exchangeName, routingKey);
        
        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            arguments: null);
    }

    public async ValueTask DisposeAsync()
    {
        // Adquire o lock para garantir que nenhuma operação concorrente esteja em andamento
        await _channelLock.WaitAsync();
        try
        {
            _disposed = true;
            
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
                _channel = null;
            }
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        finally
        {
            _channelLock.Release();
            _channelLock.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
