namespace MeAjudaAi.Shared.Messaging.RabbitMq;

/// <summary>
/// Gerencia a infraestrutura do RabbitMQ, incluindo exchanges, filas e bindings.
/// </summary>
public interface IRabbitMqInfrastructureManager
{
    /// <summary>
    /// Garante que toda a infraestrutura necessária do RabbitMQ esteja criada.
    /// </summary>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    Task EnsureInfrastructureAsync();

    /// <summary>
    /// Cria uma fila no RabbitMQ.
    /// </summary>
    /// <param name="queueName">Nome da fila a ser criada.</param>
    /// <param name="durable">Indica se a fila deve ser durável (padrão: true).</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    Task CreateQueueAsync(string queueName, bool durable = true);

    /// <summary>
    /// Cria um exchange no RabbitMQ.
    /// </summary>
    /// <param name="exchangeName">Nome do exchange a ser criado.</param>
    /// <param name="exchangeType">Tipo do exchange (padrão: "topic").</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    Task CreateExchangeAsync(string exchangeName, string exchangeType = "topic");

    /// <summary>
    /// Vincula uma fila a um exchange com uma chave de roteamento.
    /// </summary>
    /// <param name="queueName">Nome da fila a ser vinculada.</param>
    /// <param name="exchangeName">Nome do exchange.</param>
    /// <param name="routingKey">Chave de roteamento para o binding (padrão: string vazia).</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    Task BindQueueToExchangeAsync(string queueName, string exchangeName, string routingKey = "");
}
