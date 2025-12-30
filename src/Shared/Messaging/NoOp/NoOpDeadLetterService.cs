using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Implementação de fallback do serviço de Dead Letter Queue para testes
/// </summary>
public sealed class NoOpDeadLetterService(ILogger<NoOpDeadLetterService> logger) : IDeadLetterService
{
    public Task SendToDeadLetterAsync<TMessage>(
        TMessage message,
        Exception exception,
        string handlerType,
        string sourceQueue,
        int attemptCount,
        CancellationToken cancellationToken = default) where TMessage : class
    {
        logger.LogWarning(
            "NoOp: Would send message to dead letter queue. Type: {MessageType}, Queue: {Queue}, Attempts: {Attempts}, Reason: {Reason}",
            typeof(TMessage).Name, sourceQueue, attemptCount, exception.Message);

        return Task.CompletedTask;
    }

    public bool ShouldRetry(Exception exception, int attemptCount)
    {
        // Máximo 3 tentativas para NoOp (tentativas 1, 2 e 3)
        const int maxAttempts = 3;
        return attemptCount <= maxAttempts && exception.ClassifyFailure() == EFailureType.Transient;
    }

    public TimeSpan CalculateRetryDelay(int attemptCount)
    {
        // Backoff exponencial: 2^(attemptCount-1) * 2 segundos, mas com máximo de 5 minutos (300 segundos)
        var baseDelaySeconds = Math.Pow(2, attemptCount - 1) * 2;
        var maxDelaySeconds = 300; // 5 minutos
        var delaySeconds = Math.Min(baseDelaySeconds, maxDelaySeconds);
        return TimeSpan.FromSeconds(delaySeconds);
    }

    public Task ReprocessDeadLetterMessageAsync(
        string deadLetterQueueName,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("NoOp: Would reprocess message {MessageId} from dead letter queue {Queue}",
            messageId, deadLetterQueueName);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<FailedMessageInfo>> ListDeadLetterMessagesAsync(
        string deadLetterQueueName,
        int maxCount = 50,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("NoOp: Would list dead letter messages from queue {Queue}", deadLetterQueueName);
        return Task.FromResult(Enumerable.Empty<FailedMessageInfo>());
    }

    public Task PurgeDeadLetterMessageAsync(
        string deadLetterQueueName,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("NoOp: Would purge message {MessageId} from dead letter queue {Queue}",
            messageId, deadLetterQueueName);
        return Task.CompletedTask;
    }

    public Task<DeadLetterStatistics> GetDeadLetterStatisticsAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("NoOp: Would get dead letter statistics");
        return Task.FromResult(new DeadLetterStatistics());
    }
}
