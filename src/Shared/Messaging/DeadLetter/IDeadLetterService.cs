namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Interface para gerenciamento de Dead Letter Queue
/// </summary>
public interface IDeadLetterService
{
    /// <summary>
    /// Envia uma mensagem para a Dead Letter Queue
    /// </summary>
    /// <typeparam name="TMessage">Tipo da mensagem</typeparam>
    /// <param name="message">Mensagem original</param>
    /// <param name="exception">Exceção que causou a falha</param>
    /// <param name="handlerType">Tipo do handler que estava processando</param>
    /// <param name="sourceQueue">Fila de origem</param>
    /// <param name="attemptCount">Número de tentativas realizadas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task SendToDeadLetterAsync<TMessage>(
        TMessage message,
        Exception exception,
        string handlerType,
        string sourceQueue,
        int attemptCount,
        CancellationToken cancellationToken = default) where TMessage : class;

    /// <summary>
    /// Determina se uma exceção deve causar retry ou ir direto para DLQ
    /// </summary>
    /// <param name="exception">Exceção a ser analisada</param>
    /// <param name="attemptCount">Número atual de tentativas</param>
    /// <returns>True se deve tentar novamente, False se deve ir para DLQ</returns>
    bool ShouldRetry(Exception exception, int attemptCount);

    /// <summary>
    /// Calcula o delay para a próxima tentativa usando backoff exponencial
    /// </summary>
    /// <param name="attemptCount">Número da tentativa atual</param>
    /// <returns>Tempo de delay</returns>
    TimeSpan CalculateRetryDelay(int attemptCount);

    /// <summary>
    /// Reprocessa uma mensagem da Dead Letter Queue
    /// </summary>
    /// <param name="deadLetterQueueName">Nome da fila de dead letter</param>
    /// <param name="messageId">ID da mensagem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task ReprocessDeadLetterMessageAsync(
        string deadLetterQueueName,
        string messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista mensagens na Dead Letter Queue para análise
    /// </summary>
    /// <param name="deadLetterQueueName">Nome da fila de dead letter</param>
    /// <param name="maxCount">Número máximo de mensagens a retornar</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de informações das mensagens falhadas</returns>
    Task<IEnumerable<FailedMessageInfo>> ListDeadLetterMessagesAsync(
        string deadLetterQueueName,
        int maxCount = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma mensagem da Dead Letter Queue após análise
    /// </summary>
    /// <param name="deadLetterQueueName">Nome da fila de dead letter</param>
    /// <param name="messageId">ID da mensagem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task PurgeDeadLetterMessageAsync(
        string deadLetterQueueName,
        string messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém estatísticas das Dead Letter Queues
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Estatísticas das DLQs</returns>
    Task<DeadLetterStatistics> GetDeadLetterStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Estatísticas das Dead Letter Queues
/// </summary>
public sealed class DeadLetterStatistics
{
    /// <summary>
    /// Total de mensagens em todas as DLQs
    /// </summary>
    public int TotalDeadLetterMessages { get; set; }

    /// <summary>
    /// Mensagens por fila de dead letter
    /// </summary>
    public Dictionary<string, int> MessagesByQueue { get; set; } = new();

    /// <summary>
    /// Mensagens por tipo de exceção
    /// </summary>
    public Dictionary<string, int> MessagesByExceptionType { get; set; } = new();

    /// <summary>
    /// Mensagens mais antigas em cada fila
    /// </summary>
    public Dictionary<string, DateTime> OldestMessageByQueue { get; set; } = new();

    /// <summary>
    /// Taxa de falha por handler
    /// </summary>
    public Dictionary<string, FailureRate> FailureRateByHandler { get; set; } = new();

    /// <summary>
    /// Data/hora da última atualização das estatísticas
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Taxa de falha para um handler específico
/// </summary>
public sealed class FailureRate
{
    /// <summary>
    /// Número total de mensagens processadas
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// Número de mensagens que falharam
    /// </summary>
    public int FailedMessages { get; set; }

    /// <summary>
    /// Percentual de falha
    /// </summary>
    public double FailurePercentage => TotalMessages > 0 ? (double)FailedMessages / TotalMessages * 100 : 0;

    /// <summary>
    /// Última falha registrada
    /// </summary>
    public DateTime? LastFailure { get; set; }
}
