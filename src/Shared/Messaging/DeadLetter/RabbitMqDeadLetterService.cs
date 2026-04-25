using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Implementação do serviço de Dead Letter Queue usando RabbitMQ
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class RabbitMqDeadLetterService(
    RabbitMqOptions rabbitMqOptions,
    IOptions<DeadLetterOptions> deadLetterOptions,
    IMessageSerializer serializer,
    ILogger<RabbitMqDeadLetterService> logger) : IDeadLetterService, IAsyncDisposable, IDisposable
{
    private readonly DeadLetterOptions _deadLetterOptions = deadLetterOptions.Value;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private readonly CancellationTokenSource _disposeCts = new();
    private readonly ConcurrentDictionary<string, bool> _declaredQuarantineQueues = new();
    private int _disposedValue; // 0 = not disposed, 1 = disposing/disposed
    private bool _disposed => _disposedValue == 1;

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

            var messageBody = Encoding.UTF8.GetBytes(serializer.Serialize(failedMessageInfo));
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

    public async Task<bool> ReprocessDeadLetterMessageAsync(
        string deadLetterQueueName,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionAsync();

            // Buscamos na fila até encontrar a mensagem ou a fila esvaziar
            while (true)
            {
                var result = await _channel!.BasicGetAsync(deadLetterQueueName, autoAck: false, cancellationToken);
                if (result == null)
                {
                    logger.LogWarning("Message {MessageId} not found in dead letter queue {Queue}",
                        messageId, deadLetterQueueName);
                    return false;
                }

                var messageBodyJson = Encoding.UTF8.GetString(result.Body.Span);
                FailedMessageInfo? failedMessageInfo = null;

                try
                {
                    failedMessageInfo = serializer.Deserialize<FailedMessageInfo>(messageBodyJson);
                }
                catch (Exception ex)
                {
                    var bodyPreview = messageBodyJson.Length > 100 ? messageBodyJson[..100] + "..." : messageBodyJson;
                    logger.LogError(ex, "Failed to deserialize dead letter message from queue {Queue} (DeliveryTag: {DeliveryTag}). Body Preview: {BodyPreview}. Moving to quarantine.", 
                        deadLetterQueueName, result.DeliveryTag, bodyPreview);
                    
                    try
                    {
                        await SendToQuarantineAsync(deadLetterQueueName, result.Body, result.BasicProperties, cancellationToken);
                        await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
                    }
                    catch (Exception quarantineEx)
                    {
                        await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
                        logger.LogCritical(
                            quarantineEx,
                            "Critical: could not move message to quarantine. DeliveryTag: {DeliveryTag}, MessageId: {MessageId}, PayloadHash: {PayloadHash}, PayloadLength: {PayloadLength}, DeadLetterQueueName: {Queue}",
                            result.DeliveryTag,
                            result.BasicProperties.MessageId,
                            GetPayloadHash(result.Body),
                            result.Body.Length,
                            deadLetterQueueName);
                    }
                    continue; // Tenta o próximo
                }

                if (failedMessageInfo?.MessageId == messageId)
                {
                    // Reenvia para a fila original
                    var originalMessageBody = Encoding.UTF8.GetBytes(failedMessageInfo.OriginalMessage);
                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        MessageId = Guid.NewGuid().ToString(),
                        Headers = new Dictionary<string, object?>
                        {
                            ["reprocessed-from-dlq"] = true,
                            ["original-message-id"] = messageId,
                            ["reprocessed-at"] = DateTime.UtcNow.ToString("O")
                        }
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
                    
                    return true;
                }
                else
                {
                    // Rejeita a mensagem sem recolocar no início para evitar loop infinito
                    // Republicamos para o fim da fila ANTES do Ack para evitar perda
                    
                    var foundId = result.BasicProperties?.MessageId;
                    logger.LogWarning("Requested reprocess for MessageId {RequestedId}, but found {FoundId} in queue {Queue}. Republishing to tail.",
                        messageId, foundId ?? "null", deadLetterQueueName);

                    var props = result.BasicProperties;
                    var publishProperties = new BasicProperties
                    {
                        Persistent = true,
                        MessageId = props.MessageId,
                        CorrelationId = props.CorrelationId,
                        ContentType = props.ContentType,
                        ContentEncoding = props.ContentEncoding,
                        Timestamp = props.Timestamp,
                        Headers = props.Headers != null ? new Dictionary<string, object?>(props.Headers) : null,
                        Priority = props.Priority,
                        ReplyTo = props.ReplyTo,
                        Expiration = props.Expiration,
                        Type = props.Type,
                        UserId = props.UserId,
                        AppId = props.AppId,
                        ClusterId = props.ClusterId
                    };

                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: deadLetterQueueName,
                        mandatory: false,
                        basicProperties: publishProperties,
                        body: result.Body,
                        cancellationToken: cancellationToken);

                    await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
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
        var seenMessageIds = new HashSet<string>();

        try
        {
            await EnsureConnectionAsync();

            var count = 0;
            while (count < maxCount)
            {
                var result = await _channel!.BasicGetAsync(deadLetterQueueName, autoAck: false, cancellationToken);
                if (result == null) break;

                // Em modo de inspeção (list), não queremos remover mensagens da fila,
                // exceto duplicadas que já processamos nesta iteração.
                var wasAcked = false;
                var messageBodyJson = Encoding.UTF8.GetString(result.Body.Span);
                FailedMessageInfo? failedMessageInfo = null;

                try
                {
                    failedMessageInfo = serializer.Deserialize<FailedMessageInfo>(messageBodyJson);

                    // Deduplicação adicional por MessageId se disponível
                    if (failedMessageInfo?.MessageId != null && seenMessageIds.Contains(failedMessageInfo.MessageId))
                    {
                        await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
                        wasAcked = true;
                        continue;
                    }

                    if (failedMessageInfo != null)
                    {
                        messages.Add(failedMessageInfo);
                        if (failedMessageInfo.MessageId != null)
                        {
                            seenMessageIds.Add(failedMessageInfo.MessageId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to deserialize dead letter message during list operation. Queue: {Queue}", deadLetterQueueName);
                }
                finally
                {
                    if (!wasAcked)
                    {
                        // Se não foi um duplicado removido via Ack, devolvemos para a fila com Nack(requeue:true)
                        // Isso garante que a inspeção não seja destrutiva.
                        await _channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                    }
                    count++;
                }
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

    public async Task<bool> PurgeDeadLetterMessageAsync(
        string deadLetterQueueName,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectionAsync();

            while (true)
            {
                var result = await _channel!.BasicGetAsync(deadLetterQueueName, autoAck: false, cancellationToken);
                if (result == null)
                {
                    logger.LogWarning("Message {MessageId} not found in dead letter queue {Queue} for purge",
                        messageId, deadLetterQueueName);
                    return false;
                }

                var messageBodyJson = Encoding.UTF8.GetString(result.Body.Span);
                FailedMessageInfo? failedMessageInfo = null;

                try
                {
                    failedMessageInfo = serializer.Deserialize<FailedMessageInfo>(messageBodyJson);
                }
                catch (Exception ex)
                {
                    var bodyPreview = messageBodyJson.Length > 100 ? messageBodyJson[..100] + "..." : messageBodyJson;
                    logger.LogError(ex, "Failed to deserialize dead letter message from queue {Queue} (DeliveryTag: {DeliveryTag}). Body Preview: {BodyPreview}. Moving to quarantine.", 
                        deadLetterQueueName, result.DeliveryTag, bodyPreview);
                    
                    try
                    {
                        await SendToQuarantineAsync(deadLetterQueueName, result.Body, result.BasicProperties, cancellationToken);
                        await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
                    }
                    catch (Exception quarantineEx)
                    {
                        await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
                        logger.LogCritical(
                            quarantineEx,
                            "Critical: could not move message to quarantine during purge. DeliveryTag: {DeliveryTag}, MessageId: {MessageId}, PayloadHash: {PayloadHash}, PayloadLength: {PayloadLength}, DeadLetterQueueName: {Queue}",
                            result.DeliveryTag,
                            result.BasicProperties.MessageId,
                            GetPayloadHash(result.Body),
                            result.Body.Length,
                            deadLetterQueueName);
                    }
                    continue;
                }

                if (failedMessageInfo?.MessageId == messageId)
                {
                    await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
                    logger.LogInformation("Dead letter message {MessageId} purged from queue {Queue}",
                        messageId, deadLetterQueueName);
                    
                    return true;
                }
                else
                {
                    // Rejeita a mensagem sem recolocar no início para evitar loop infinito
                    // Republicamos para o fim da fila ANTES do Ack para evitar perda
                    
                    var foundId = result.BasicProperties?.MessageId;
                    logger.LogWarning("Requested purge for MessageId {RequestedId}, but found {FoundId} in queue {Queue}. Republishing to tail.",
                        messageId, foundId ?? "null", deadLetterQueueName);

                    var props = result.BasicProperties;
                    var publishProperties = new BasicProperties
                    {
                        Persistent = true,
                        MessageId = props.MessageId,
                        CorrelationId = props.CorrelationId,
                        ContentType = props.ContentType,
                        ContentEncoding = props.ContentEncoding,
                        Timestamp = props.Timestamp,
                        Headers = props.Headers != null ? new Dictionary<string, object?>(props.Headers) : null,
                        Priority = props.Priority,
                        ReplyTo = props.ReplyTo,
                        Expiration = props.Expiration,
                        Type = props.Type,
                        UserId = props.UserId,
                        AppId = props.AppId,
                        ClusterId = props.ClusterId
                    };

                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: deadLetterQueueName,
                        mandatory: false,
                        basicProperties: publishProperties,
                        body: result.Body,
                        cancellationToken: cancellationToken);

                    await _channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        await _connectionSemaphore.WaitAsync(_disposeCts.Token);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
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

                // Limpa o cache de filas declaradas quando o canal é recriado
                _declaredQuarantineQueues.Clear();
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
            OriginalMessage = serializer.Serialize(message),
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

    private async Task SendToQuarantineAsync(
        string deadLetterQueueName,
        ReadOnlyMemory<byte> body,
        IReadOnlyBasicProperties properties,
        CancellationToken cancellationToken)
    {
        var quarantineQueue = $"{deadLetterQueueName}.quarantine";
        
        try
        {
            // Evitamos declarações redundantes via cache em memória (race condition resolvida via idempotência do RMQ)
            if (!_declaredQuarantineQueues.ContainsKey(quarantineQueue))
            {
                var args = new Dictionary<string, object?>
                {
                    ["x-message-ttl"] = (int)TimeSpan.FromDays(30).TotalMilliseconds,
                    ["x-max-length"] = 10000, // Limite de 10k mensagens
                    ["x-overflow"] = "reject-publish"
                };

                await _channel!.QueueDeclareAsync(
                    queue: quarantineQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: args,
                    cancellationToken: cancellationToken);
                
                _declaredQuarantineQueues.TryAdd(quarantineQueue, true);
            }

            var props = properties;
            var publishProperties = new BasicProperties
            {
                Persistent = true,
                MessageId = props.MessageId,
                CorrelationId = props.CorrelationId,
                ContentType = props.ContentType,
                ContentEncoding = props.ContentEncoding,
                Timestamp = props.Timestamp,
                Headers = props.Headers != null ? new Dictionary<string, object?>(props.Headers) : new Dictionary<string, object?>()
            };

            // Estende headers com metadados de quarentena
            var headers = publishProperties.Headers!;
            headers["x-quarantine-reason"] = "deserialization_failure";
            headers["x-original-queue"] = deadLetterQueueName;
            headers["x-quarantined-at"] = DateTime.UtcNow.ToString("O");

            await _channel!.BasicPublishAsync(
                exchange: "",
                routingKey: quarantineQueue,
                mandatory: false,
                basicProperties: publishProperties,
                body: body,
                cancellationToken: cancellationToken);

            logger.LogWarning("Corrupt dead letter message moved to quarantine queue: {Queue}. Metric: dead_letter_quarantined_total=1", quarantineQueue);
        }
        catch (Exception ex)
        {
            // Se falhou ao declarar, removemos do cache para tentar novamente na próxima
            _declaredQuarantineQueues.TryRemove(quarantineQueue, out _);

            logger.LogError(ex, "Critical failure: could not move corrupt message to quarantine queue {Queue}", quarantineQueue);
            throw; // Re-lança para forçar Nack com requeue se o chamador tratar
        }
    }

    private string GetDeadLetterQueueName(string sourceQueue)
    {
        return $"{_deadLetterOptions.DeadLetterQueuePrefix}.{sourceQueue}";
    }

    private string GetDeadLetterRoutingKey(string sourceQueue)
    {
        return $"{_deadLetterOptions.RabbitMq.DeadLetterRoutingKey}.{sourceQueue}";
    }

    private static string GetPayloadHash(ReadOnlyMemory<byte> body)
    {
        var hashBytes = SHA256.HashData(body.Span);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
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
        if (Interlocked.Exchange(ref _disposedValue, 1) == 1) return;

        try
        {
            await _disposeCts.CancelAsync();
            _disposeCts.Dispose();

            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
                _channel = null;
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing RabbitMQ dead letter service");
        }
        finally
        {
            _connectionSemaphore?.Dispose();
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Limpeza síncrona: libera o semáforo e anula referências sem bloquear em código assíncrono.
    /// Recursos de rede (channel/connection) podem não ser fechados graciosamente; prefira DisposeAsync.
    /// </remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposedValue, 1) == 1) return;

        try
        {
            _disposeCts.Cancel();
            _disposeCts.Dispose();
            _connectionSemaphore?.Dispose();

            _channel?.Dispose();
            _connection?.Dispose();
            _channel = null;
            _connection = null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing RabbitMQ dead letter service (sync)");
        }
    }
}
