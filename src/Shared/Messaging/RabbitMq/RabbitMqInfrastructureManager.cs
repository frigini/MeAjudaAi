using MeAjudaAi.Shared.Messaging.Strategy;
using MeAjudaAi.Shared.Messaging.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MeAjudaAi.Shared.Messaging.RabbitMq;

public interface IRabbitMqInfrastructureManager
{
    Task EnsureInfrastructureAsync();
    Task CreateQueueAsync(string queueName, bool durable = true);
    Task CreateExchangeAsync(string exchangeName, string exchangeType = ExchangeType.Topic);
    Task BindQueueToExchangeAsync(string queueName, string exchangeName, string routingKey = "");
}

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
            _logger.LogInformation("Criando infraestrutura RabbitMQ...");

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

                _logger.LogDebug("Infraestrutura criada para o tipo de evento {EventType}: exchange={Exchange}, queue={Queue}",
                    eventType.Name, exchangeName, queueName);
            }

            _logger.LogInformation("Infraestrutura RabbitMQ criada com sucesso");
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
        _logger.LogDebug("Solicitada criação de fila: {QueueName} (durável: {Durable})", queueName, durable);
        return Task.CompletedTask;
    }

    public Task CreateExchangeAsync(string exchangeName, string exchangeType = ExchangeType.Topic)
    {
        // Implementação RabbitMQ será adicionada quando necessário
        _logger.LogDebug("Solicitada criação de exchange: {ExchangeName} (tipo: {ExchangeType})", exchangeName, exchangeType);
        return Task.CompletedTask;
    }

    public Task BindQueueToExchangeAsync(string queueName, string exchangeName, string routingKey = "")
    {
        // Implementação RabbitMQ será adicionada quando necessário
        _logger.LogDebug("Solicitada vinculação de fila: {QueueName} para {ExchangeName} com chave '{RoutingKey}'",
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
