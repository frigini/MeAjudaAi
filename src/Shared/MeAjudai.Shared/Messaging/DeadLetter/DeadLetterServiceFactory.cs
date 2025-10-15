using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Factory para criar o serviço de Dead Letter Queue apropriado baseado no ambiente
/// </summary>
public interface IDeadLetterServiceFactory
{
    /// <summary>
    /// Cria o serviço de DLQ apropriado para o ambiente atual
    /// </summary>
    IDeadLetterService CreateDeadLetterService();
}

/// <summary>
/// Implementação do factory que seleciona o serviço de DLQ baseado no ambiente:
/// - Development/Testing: Serviço RabbitMQ Dead Letter
/// - Production: Serviço Service Bus Dead Letter
/// </summary>
public sealed class EnvironmentBasedDeadLetterServiceFactory(
    IServiceProvider serviceProvider,
    IHostEnvironment environment,
    ILogger<EnvironmentBasedDeadLetterServiceFactory> logger) : IDeadLetterServiceFactory
{
    public IDeadLetterService CreateDeadLetterService()
    {
        if (environment.IsDevelopment() || environment.EnvironmentName == "Testing")
        {
            logger.LogInformation("Creating RabbitMQ Dead Letter Service for environment: {Environment}", environment.EnvironmentName);
            return serviceProvider.GetRequiredService<RabbitMqDeadLetterService>();
        }
        else
        {
            logger.LogInformation("Creating Service Bus Dead Letter Service for environment: {Environment}", environment.EnvironmentName);
            return serviceProvider.GetRequiredService<ServiceBusDeadLetterService>();
        }
    }
}

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
        // Abordagem conservativa para NoOp - não tenta muitas vezes
        return attemptCount < 2 && exception.ClassifyFailure() == EFailureType.Transient;
    }

    public TimeSpan CalculateRetryDelay(int attemptCount)
    {
        return TimeSpan.FromSeconds(Math.Pow(2, attemptCount)); // Backoff exponencial simples
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
