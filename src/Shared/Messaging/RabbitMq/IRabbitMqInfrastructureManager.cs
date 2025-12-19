namespace MeAjudaAi.Shared.Messaging.RabbitMq;

public interface IRabbitMqInfrastructureManager
{
    Task EnsureInfrastructureAsync();
    Task CreateQueueAsync(string queueName, bool durable = true);
    Task CreateExchangeAsync(string exchangeName, string exchangeType = "topic");
    Task BindQueueToExchangeAsync(string queueName, string exchangeName, string routingKey = "");
}
