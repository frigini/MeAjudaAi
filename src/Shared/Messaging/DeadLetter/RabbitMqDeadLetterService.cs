using System.Text;
using System.Text.Json;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Implementação do serviço de Dead Letter Queue usando RabbitMQ
/// </summary>
public sealed class RabbitMqDeadLetterService(
    RabbitMqOptions rabbitMqOptions,
    IOptions<DeadLetterOptions> deadLetterOptions,
    ILogger<RabbitMqDeadLetterService> logger) : IDeadLetterService, IAsyncDisposable
{
    private readonly DeadLetterOptions _deadLetterOptions = deadLetterOptions.Value;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    public async Task SendToDeadLetterAsync<TMessage>(
        TMessage message,
        Exception exception,
        string handlerType,
        string sourceQueue,
        int attemptCount,
        CancellationToken cancellationToken = default) where TMessage : class
    {
        string? messageId = null;
        string? messageType = null;
        int capturedAttemptCount = attemptCount;

        try
        {
            var failedMessageInfo = CreateFailedMessageInfo(message, exception, handlerType, sourceQueue, attemptCount);
            messageId = failedMessageInfo.MessageId;
            messageType = failedMessageInfo.MessageType;
            var deadLetterQueueName = GetDeadLetterQueueName(sourceQueue);

            await EnsureConnectionAsync();
            await EnsureDeadLetterInfrastructureAsync(deadLetterQueueName);

            var messageBody = Encoding.UTF8.GetBytes(failedMessageInfo.ToJson());
            var properties = new BasicProperties
            {
                Persistent = _deadLetterOptions.RabbitMq.EnablePersistence,
                MessageId = failedMessageInfo.MessageId,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Expiration = TimeSpan.FromHours(_deadLetterOptions.DeadLetterTtlHours).TotalMilliseconds.ToString()
            };

            // Adicionar headers para facilitar consultas
            properties.Headers = new Dictionary<string, object?>
            {
                ["original-message-type"] = typeof(TMessage).FullName ?? "Unknown",
                ["failure-reason"] = exception.GetType().Name,
                ["attempt-count"] = attemptCount,
                ["source-queue"] = sourceQueue,
                ["handler-type"] = handlerType,
                ["failed-at"] = DateTime.UtcNow.ToString("O")
            };

            await _channel!.BasicPublishAsync(
                exchange: _deadLetterOptions.RabbitMq.DeadLetterExchange,
                routingKey: GetDeadLetterRoutingKey(sourceQueue),
                mandatory: false,
                basicProperties: properties,
                body: messageBody,
                cancellationToken: cancellationToken);

            logger.LogWarning(
                "Message sent to dead letter queue. MessageId: {MessageId}, Type: {MessageType}, Queue: {Queue}, Attempts: {Attempts}, Reason: {Reason}",
                failedMessageInfo.MessageId, typeof(TMessage).Name, deadLetterQueueName, attemptCount, exception.Message);

            if (_deadLetterOptions.EnableAdminNotifications)
            {
                await NotifyAdministratorsAsync(failedMessageInfo);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to RabbitMQ dead letter queue. MessageId: {MessageId}, Type: {MessageType}, Attempts: {Attempts}",
                messageId ?? "unknown", messageType ?? typeof(TMessage).Name, capturedAttemptCount);
            throw new InvalidOperationException(
                $"Failed to send message '{messageId ?? "unknown"}' of type '{messageType ?? typeof(TMessage).Name}' to RabbitMQ dead letter queue after {capturedAttemptCount} attempts",
                ex);
        }
    }

    public bool ShouldRetry(Exception exception, int attemptCount)
    {
        if (attemptCount >= _deadLetterOptions.MaxRetryAttempts)
            return false;

        var failureType = exception.ClassifyFailure();

        return failureType switch
        {
            EFailureType.Permanent => false,
            EFailureType.Critical => false,
            EFailureType.Transient => true,
            EFailureType.Unknown => attemptCount < _deadLetterOptions.MaxRetryAttempts / 2,
            _ => false
        };
    }

    public TimeSpan CalculateRetryDelay(int attemptCount)
    {
        var baseDelay = TimeSpan.FromSeconds(_deadLetterOptions.InitialRetryDelaySeconds);
        var exponentialDelay = TimeSpan.FromSeconds(baseDelay.TotalSeconds * Math.Pow(_deadLetterOptions.BackoffMultiplier, attemptCount - 1));
        var maxDelay = TimeSpan.FromSeconds(_deadLetterOptions.MaxRetryDelaySeconds);

        return exponentialDelay > maxDelay ? maxDelay : exponentialDelay;
    }

    public async Task ReprocessDeadLetterMessageAsync(
        string deadLetterQueueName,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionAsync();

            var result = await _channel!.BasicGetAsync(deadLetterQueueName, autoAck: false, cancellationToken);
            if (result != null)
            {
                var messageBodyJson = Encoding.UTF8.GetString(result.Body.Span);
                var failedMessageInfo = FailedMessageInfoExtensions.FromJson(messageBodyJson);

                if (failedMessageInfo?.MessageId == messageId)
                {
                    // Reenvia para a fila original
                    var originalMessageBody = Encoding.UTF8.GetBytes(failedMessageInfo.OriginalMessage);
                    var properties = new BasicProperties();
                    properties.MessageId = Guid.NewGuid().ToString();
                    properties.Headers = new Dictionary<string, object?>
                    {
                        ["reprocessed-from-dlq"] = true,
                        ["original-message-id"] = messageId,
                        ["reprocessed-at"] = DateTime.UtcNow.ToString("O")
                    };

                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: failedMessageInfo.SourceQueue,
                        mandatory: false,
                        basicProperties: properties,
                        body: originalMessageBody,
                        cancellationToken: cancellationToken);

                    // Remove da DLQ
                    await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);

                    logger.LogInformation("Message {MessageId} reprocessed from dead letter queue {Queue}",
                        messageId, deadLetterQueueName);
                }
                else
                {
                    // Rejeita a mensagem de volta para a fila
                    await _channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reprocess dead letter message {MessageId} from queue {Queue}",
                messageId, deadLetterQueueName);
            throw new InvalidOperationException(
                $"Failed to reprocess dead letter message '{messageId}' from RabbitMQ queue '{deadLetterQueueName}'",
                ex);
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
            await EnsureConnectionAsync();

            var count = 0;
            while (count < maxCount)
            {
                var result = await _channel!.BasicGetAsync(deadLetterQueueName, autoAck: false, cancellationToken);
                if (result == null) break;

                var messageBodyJson = Encoding.UTF8.GetString(result.Body.Span);
                var failedMessageInfo = FailedMessageInfoExtensions.FromJson(messageBodyJson);

                if (failedMessageInfo != null)
                {
                    messages.Add(failedMessageInfo);
                }

                // Importante: Rejeita a mensagem de volta para a fila
                await _channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                count++;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list dead letter messages from queue {Queue}", deadLetterQueueName);
            throw new InvalidOperationException(
                $"Failed to list dead letter messages from RabbitMQ queue '{deadLetterQueueName}'",
                ex);
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
            await EnsureConnectionAsync();

            var result = await _channel!.BasicGetAsync(deadLetterQueueName, autoAck: false, cancellationToken);
            if (result != null)
            {
                var messageBodyJson = Encoding.UTF8.GetString(result.Body.Span);
                var failedMessageInfo = FailedMessageInfoExtensions.FromJson(messageBodyJson);

                if (failedMessageInfo?.MessageId == messageId)
                {
                    await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
                    logger.LogInformation("Dead letter message {MessageId} purged from queue {Queue}",
                        messageId, deadLetterQueueName);
                }
                else
                {
                    await _channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to purge dead letter message {MessageId} from queue {Queue}",
                messageId, deadLetterQueueName);
            throw new InvalidOperationException(
                $"Failed to purge dead letter message '{messageId}' from RabbitMQ queue '{deadLetterQueueName}'",
                ex);
        }
    }

    public async Task<DeadLetterStatistics> GetDeadLetterStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var statistics = new DeadLetterStatistics();

        try
        {
            await EnsureConnectionAsync();

            // Coleta estatísticas básicas das filas DLQ conhecidas
            var deadLetterQueues = GetKnownDeadLetterQueues();

            foreach (var queueName in deadLetterQueues)
            {
                try
                {
                    var queueInfo = await _channel!.QueueDeclarePassiveAsync(queueName, cancellationToken);
                    statistics.MessagesByQueue[queueName] = (int)queueInfo.MessageCount;
                    statistics.TotalDeadLetterMessages += (int)queueInfo.MessageCount;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to get statistics for dead letter queue {Queue}", queueName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get dead letter statistics");
            throw new InvalidOperationException(
                "Failed to retrieve RabbitMQ dead letter queue statistics (message counts, queue names)",
                ex);
        }

        return statistics;
    }

    private async Task EnsureConnectionAsync()
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        await _connectionSemaphore.WaitAsync();
        try
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
                return;

            try
            {
                var factory = new ConnectionFactory();
                if (!string.IsNullOrWhiteSpace(rabbitMqOptions.ConnectionString))
                {
                    factory.Uri = new Uri(rabbitMqOptions.ConnectionString);
                }
                else
                {
                    factory.HostName = rabbitMqOptions.Host;
                    factory.Port = rabbitMqOptions.Port;
                    factory.UserName = rabbitMqOptions.Username;
                    factory.Password = rabbitMqOptions.Password;
                    factory.VirtualHost = rabbitMqOptions.VirtualHost;
                }

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create RabbitMQ connection for dead letter service");
                throw new InvalidOperationException(
                    $"Failed to create RabbitMQ connection for dead letter service (host: {rabbitMqOptions.Host}:{rabbitMqOptions.Port})",
                    ex);
            }
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    private async Task EnsureDeadLetterInfrastructureAsync(string deadLetterQueueName)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel not available");

        // Declara o exchange de dead letter
        await _channel.ExchangeDeclareAsync(
            exchange: _deadLetterOptions.RabbitMq.DeadLetterExchange,
            type: ExchangeType.Topic,
            durable: true);

        // Declara a fila de dead letter
        var arguments = new Dictionary<string, object?>();
        if (_deadLetterOptions.DeadLetterTtlHours > 0)
        {
            arguments["x-message-ttl"] = (int)TimeSpan.FromHours(_deadLetterOptions.DeadLetterTtlHours).TotalMilliseconds;
        }

        await _channel.QueueDeclareAsync(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments);

        // Vincula a fila ao exchange
        await _channel.QueueBindAsync(
            queue: deadLetterQueueName,
            exchange: _deadLetterOptions.RabbitMq.DeadLetterExchange,
            routingKey: GetDeadLetterRoutingKey(deadLetterQueueName));
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
                ApplicationVersion = typeof(RabbitMqDeadLetterService).Assembly.GetName().Version?.ToString() ?? "Unknown",
                ServiceInstance = Environment.MachineName
            }
        };

        failedMessageInfo.AddFailureAttempt(exception, handlerType);

        return failedMessageInfo;
    }

    private string GetDeadLetterQueueName(string sourceQueue)
    {
        return $"{_deadLetterOptions.DeadLetterQueuePrefix}.{sourceQueue}";
    }

    private string GetDeadLetterRoutingKey(string sourceQueue)
    {
        return $"{_deadLetterOptions.RabbitMq.DeadLetterRoutingKey}.{sourceQueue}";
    }

    private List<string> GetKnownDeadLetterQueues()
    {
        // Retorna as filas DLQ conhecidas baseadas nas filas de domínio configuradas
        var deadLetterQueues = new List<string>();

        foreach (var domainQueue in rabbitMqOptions.DomainQueues)
        {
            deadLetterQueues.Add(GetDeadLetterQueueName(domainQueue.Value));
        }

        deadLetterQueues.Add(GetDeadLetterQueueName(rabbitMqOptions.DefaultQueueName));

        return deadLetterQueues;
    }

    private Task NotifyAdministratorsAsync(FailedMessageInfo failedMessageInfo)
    {
        try
        {
            // TODO(#247): Implement administrator notifications for RabbitMQ dead letter queue threshold.
            // Strategy: Use IEmailService + RabbitMQ Management API for queue metrics.
            // Threshold: Configure via DeadLetterOptions.MaxMessagesBeforeAlert (default: 100).
            // Can query queue message count using RabbitMQ HTTP API: GET /api/queues/{vhost}/{queue}
            // Could integrate: Email, Slack webhook, Microsoft Teams, or monitoring alerts.
            logger.LogWarning(
                "Admin notification: Message {MessageId} of type {MessageType} failed {AttemptCount} times and was sent to DLQ",
                failedMessageInfo.MessageId, failedMessageInfo.MessageType, failedMessageInfo.AttemptCount);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to notify administrators about dead letter message {MessageId}",
                failedMessageInfo.MessageId);
            return Task.CompletedTask;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            _connectionSemaphore?.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing RabbitMQ dead letter service");
        }
    }
}
