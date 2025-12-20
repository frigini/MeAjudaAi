using MeAjudaAi.Shared.Messaging.Strategy;
using MeAjudaAi.Shared.Messaging.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MeAjudaAi.Shared.Messaging.RabbitMq;

internal class RabbitMqInfrastructureManager : IRabbitMqInfrastructureManager, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly IEventTypeRegistry _eventRegistry;
    private readonly ITopicStrategySelector _topicSelector;
    private readonly ILogger<RabbitMqInfrastructureManager> _logger;

    public RabbitMqInfrastructureManager(
        RabbitMqOptions options,
        IEventTypeRegistry eventRegistry,
        ITopicStrategySelector topicSelector,
        ILogger<RabbitMqInfrastructureManager> logger)
    {
        _options = options;
        _eventRegistry = eventRegistry;
        _topicSelector = topicSelector;
        _logger = logger;
    }

    public async Task EnsureInfrastructureAsync()
    {
        try
        {
            _logger.LogInformation("Creating RabbitMQ infrastructure...");

            // Cria fila padrão
            await CreateQueueAsync(_options.DefaultQueueName);

            // Cria filas específicas de domínio
            foreach (var domainQueue in _options.DomainQueues)
            {
                await CreateQueueAsync(domainQueue.Value);
            }

            // Cria exchanges e bindings para tipos de eventos
            var eventTypes = await _eventRegistry.GetAllEventTypesAsync();
            foreach (var eventType in eventTypes)
            {
                var queueName = _topicSelector.SelectTopicForEvent(eventType);
                var exchangeName = $"{queueName}.exchange";

                await CreateExchangeAsync(exchangeName, ExchangeType.Topic);
                await CreateQueueAsync(queueName);
                await BindQueueToExchangeAsync(queueName, exchangeName, eventType.Name);

                _logger.LogDebug("Infrastructure created for event type {EventType}: exchange={Exchange}, queue={Queue}",
                    eventType.Name, exchangeName, queueName);
            }

            _logger.LogWarning("RabbitMQ infrastructure setup completed (stub implementation - actual infrastructure pending)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ infrastructure");
            throw new InvalidOperationException(
                "Failed to create RabbitMQ infrastructure (exchanges, queues, and bindings for registered event types)",
                ex);
        }
    }

    public Task CreateQueueAsync(string queueName, bool durable = true)
    {
        // Implementação RabbitMQ será adicionada quando necessário
        _logger.LogDebug("Queue creation requested: {QueueName} (durable: {Durable})", queueName, durable);
        return Task.CompletedTask;
    }

    public Task CreateExchangeAsync(string exchangeName, string exchangeType = ExchangeType.Topic)
    {
        // Implementação RabbitMQ será adicionada quando necessário
        _logger.LogDebug("Exchange creation requested: {ExchangeName} (type: {ExchangeType})", exchangeName, exchangeType);
        return Task.CompletedTask;
    }

    public Task BindQueueToExchangeAsync(string queueName, string exchangeName, string routingKey = "")
    {
        // Implementação RabbitMQ será adicionada quando necessário
        _logger.LogDebug("Queue binding requested: {QueueName} to {ExchangeName} with routing key '{RoutingKey}'",
            queueName, exchangeName, routingKey);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        // Dispose será implementado quando a conexão RabbitMQ for adicionada
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
