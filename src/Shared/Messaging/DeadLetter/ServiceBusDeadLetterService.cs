using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Implementação do serviço de Dead Letter Queue usando Azure Service Bus
/// </summary>
public sealed class ServiceBusDeadLetterService(
    Azure.Messaging.ServiceBus.ServiceBusClient client,
    IOptions<DeadLetterOptions> options,
    ILogger<ServiceBusDeadLetterService> logger) : IDeadLetterService
{
    private readonly DeadLetterOptions _options = options.Value;

    public async Task SendToDeadLetterAsync<TMessage>(
        TMessage message,
        Exception exception,
        string handlerType,
        string sourceQueue,
        int attemptCount,
        CancellationToken cancellationToken = default) where TMessage : class
    {
        try
        {
            var failedMessageInfo = CreateFailedMessageInfo(message, exception, handlerType, sourceQueue, attemptCount);

            var deadLetterQueueName = GetDeadLetterQueueName(sourceQueue);
            var sender = client.CreateSender(deadLetterQueueName);

            var serviceBusMessage = new Azure.Messaging.ServiceBus.ServiceBusMessage(failedMessageInfo.ToJson())
            {
                MessageId = failedMessageInfo.MessageId,
                Subject = $"DeadLetter-{typeof(TMessage).Name}",
                TimeToLive = TimeSpan.FromHours(_options.DeadLetterTtlHours)
            };

            // Adiciona propriedades para facilitar consultas
            serviceBusMessage.ApplicationProperties["OriginalMessageType"] = typeof(TMessage).FullName;
            serviceBusMessage.ApplicationProperties["FailureReason"] = exception.GetType().Name;
            serviceBusMessage.ApplicationProperties["AttemptCount"] = attemptCount;
            serviceBusMessage.ApplicationProperties["SourceQueue"] = sourceQueue;
            serviceBusMessage.ApplicationProperties["HandlerType"] = handlerType;
            serviceBusMessage.ApplicationProperties["FailedAt"] = DateTime.UtcNow;

            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            logger.LogWarning(
                "Message sent to dead letter queue. MessageId: {MessageId}, Type: {MessageType}, Queue: {Queue}, Attempts: {Attempts}, Reason: {Reason}",
                failedMessageInfo.MessageId, typeof(TMessage).Name, deadLetterQueueName, attemptCount, exception.Message);

            if (_options.EnableAdminNotifications)
            {
                await NotifyAdministratorsAsync(failedMessageInfo, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to dead letter queue. Original exception: {OriginalException}", exception.Message);
            throw;
        }
    }

    public bool ShouldRetry(Exception exception, int attemptCount)
    {
        if (attemptCount >= _options.MaxRetryAttempts)
            return false;

        var failureType = exception.ClassifyFailure();

        return failureType switch
        {
            EFailureType.Permanent => false,
            EFailureType.Critical => false,
            EFailureType.Transient => true,
            EFailureType.Unknown => attemptCount < _options.MaxRetryAttempts / 2, // Retry conservativo para falhas desconhecidas
            _ => false
        };
    }

    public TimeSpan CalculateRetryDelay(int attemptCount)
    {
        var baseDelay = TimeSpan.FromSeconds(_options.InitialRetryDelaySeconds);
        var exponentialDelay = TimeSpan.FromSeconds(baseDelay.TotalSeconds * Math.Pow(_options.BackoffMultiplier, attemptCount - 1));
        var maxDelay = TimeSpan.FromSeconds(_options.MaxRetryDelaySeconds);

        return exponentialDelay > maxDelay ? maxDelay : exponentialDelay;
    }

    public async Task ReprocessDeadLetterMessageAsync(
        string deadLetterQueueName,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var receiver = client.CreateReceiver(deadLetterQueueName);
            var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(30), cancellationToken);

            if (message?.MessageId == messageId)
            {
                var failedMessageInfo = FailedMessageInfoExtensions.FromJson(message.Body.ToString());
                if (failedMessageInfo != null)
                {
                    // Reenvia para a fila original
                    var originalQueueSender = client.CreateSender(failedMessageInfo.SourceQueue);
                    var reprocessMessage = new Azure.Messaging.ServiceBus.ServiceBusMessage(failedMessageInfo.OriginalMessage)
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        Subject = "Reprocessed"
                    };

                    await originalQueueSender.SendMessageAsync(reprocessMessage, cancellationToken);
                    await receiver.CompleteMessageAsync(message, cancellationToken);

                    logger.LogInformation("Message {MessageId} reprocessed from dead letter queue {Queue}",
                        messageId, deadLetterQueueName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reprocess dead letter message {MessageId} from queue {Queue}",
                messageId, deadLetterQueueName);
            throw;
        }
    }

    public async Task<IEnumerable<FailedMessageInfo>> ListDeadLetterMessagesAsync(
        string deadLetterQueueName,
        int maxCount = 50,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<FailedMessageInfo>();

        try
        {
            var receiver = client.CreateReceiver(deadLetterQueueName);
            var receivedMessages = await receiver.ReceiveMessagesAsync(maxCount, TimeSpan.FromSeconds(30), cancellationToken);

            foreach (var message in receivedMessages)
            {
                var failedMessageInfo = FailedMessageInfoExtensions.FromJson(message.Body.ToString());
                if (failedMessageInfo != null)
                {
                    messages.Add(failedMessageInfo);
                }

                // Importante: Abandona a mensagem para não removê-la da fila
                await receiver.AbandonMessageAsync(message, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list dead letter messages from queue {Queue}", deadLetterQueueName);
            throw;
        }

        return messages;
    }

    public async Task PurgeDeadLetterMessageAsync(
        string deadLetterQueueName,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var receiver = client.CreateReceiver(deadLetterQueueName);
            var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(30), cancellationToken);

            if (message?.MessageId == messageId)
            {
                await receiver.CompleteMessageAsync(message, cancellationToken);
                logger.LogInformation("Dead letter message {MessageId} purged from queue {Queue}",
                    messageId, deadLetterQueueName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to purge dead letter message {MessageId} from queue {Queue}",
                messageId, deadLetterQueueName);
            throw;
        }
    }

    public async Task<DeadLetterStatistics> GetDeadLetterStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var statistics = new DeadLetterStatistics();

        try
        {
            // Esta implementação é básica - em produção, você poderia usar Service Bus Management API
            // para obter estatísticas mais detalhadas das filas
            logger.LogInformation("Getting dead letter statistics - basic implementation");

            // TODO: Implementar coleta real de estatísticas usando Service Bus Management API
            // Por exemplo: ServiceBusAdministrationClient para obter propriedades das filas

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get dead letter statistics");
            throw;
        }

        return statistics;
    }

    private FailedMessageInfo CreateFailedMessageInfo<TMessage>(
        TMessage message,
        Exception exception,
        string handlerType,
        string sourceQueue,
        int attemptCount) where TMessage : class
    {
        var failedMessageInfo = new FailedMessageInfo
        {
            MessageId = Guid.NewGuid().ToString(),
            MessageType = typeof(TMessage).FullName ?? "Unknown",
            OriginalMessage = JsonSerializer.Serialize(message),
            SourceQueue = sourceQueue,
            FirstAttemptAt = DateTime.UtcNow.AddMinutes(-attemptCount * 2), // Estimativa
            LastAttemptAt = DateTime.UtcNow,
            AttemptCount = attemptCount,
            Environment = new EnvironmentMetadata
            {
                EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                ApplicationVersion = typeof(ServiceBusDeadLetterService).Assembly.GetName().Version?.ToString() ?? "Unknown",
                ServiceInstance = Environment.MachineName
            }
        };

        failedMessageInfo.AddFailureAttempt(exception, handlerType);

        return failedMessageInfo;
    }

    private string GetDeadLetterQueueName(string sourceQueue)
    {
        return $"{sourceQueue}{_options.ServiceBus.DeadLetterQueueSuffix}";
    }

    private async Task NotifyAdministratorsAsync(FailedMessageInfo failedMessageInfo, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implementar notificação para administradores
            // Isso poderia ser um email, Slack, Teams, etc.
            logger.LogWarning(
                "Admin notification: Message {MessageId} of type {MessageType} failed {AttemptCount} times and was sent to DLQ",
                failedMessageInfo.MessageId, failedMessageInfo.MessageType, failedMessageInfo.AttemptCount);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to notify administrators about dead letter message {MessageId}",
                failedMessageInfo.MessageId);
        }
    }
}
