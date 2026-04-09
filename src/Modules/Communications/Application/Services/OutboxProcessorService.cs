using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Services;

/// <summary>
/// Serviço de processamento das mensagens do Outbox.
/// </summary>
public interface IOutboxProcessorService
{
    /// <summary>
    /// Processa mensagens pendentes no Outbox.
    /// </summary>
    Task<int> ProcessPendingMessagesAsync(
        int batchSize = 50,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação do processador de Outbox.
/// </summary>
internal sealed class OutboxProcessorService(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogRepository logRepository,
    IEmailSender emailSender,
    ISmsSender smsSender,
    IPushSender pushSender,
    ILogger<OutboxProcessorService> logger) : IOutboxProcessorService
{
    private readonly ICommunicationLogRepository _logRepository = logRepository;

    public async Task<int> ProcessPendingMessagesAsync(
        int batchSize = 50,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var messages = await outboxRepository.GetPendingAsync(batchSize, utcNow, cancellationToken);

        if (messages.Count == 0)
        {
            logger.LogDebug("No pending outbox messages to process.");
            return 0;
        }

        logger.LogInformation("Processing {Count} pending outbox messages.", messages.Count);

        int processed = 0;

        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var success = await DispatchMessageAsync(message, cancellationToken);

                if (success)
                {
                    message.MarkAsSent(DateTime.UtcNow);
                    
                    var log = CommunicationLog.CreateSuccess(
                        correlationId: $"outbox:{message.Id}",
                        channel: message.Channel,
                        recipient: ExtractRecipient(message),
                        attemptCount: message.RetryCount,
                        outboxMessageId: message.Id);
                    
                    await _logRepository.AddAsync(log, cancellationToken);
                    
                    processed++;
                    logger.LogInformation("Outbox message {Id} ({Channel}) sent successfully.", message.Id, message.Channel);
                }
                else
                {
                    message.MarkAsFailed("Dispatch returned false.");
                    logger.LogWarning("Outbox message {Id} dispatch returned false. Retry {Retry}/{Max}.",
                        message.Id, message.RetryCount, message.MaxRetries);
                    
                    if (!message.HasRetriesLeft)
                    {
                        var log = CommunicationLog.CreateFailure(
                            correlationId: $"outbox:{message.Id}",
                            channel: message.Channel,
                            recipient: ExtractRecipient(message),
                            errorMessage: "Dispatch returned false and retries exhausted.",
                            attemptCount: message.RetryCount,
                            outboxMessageId: message.Id);
                        await _logRepository.AddAsync(log, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                message.MarkAsFailed(ex.Message);
                logger.LogError(ex, "Error processing outbox message {Id} ({Channel}).", message.Id, message.Channel);

                if (!message.HasRetriesLeft)
                {
                    var log = CommunicationLog.CreateFailure(
                        correlationId: $"outbox:{message.Id}",
                        channel: message.Channel,
                        recipient: ExtractRecipient(message),
                        errorMessage: ex.Message,
                        attemptCount: message.RetryCount,
                        outboxMessageId: message.Id);
                    await _logRepository.AddAsync(log, cancellationToken);
                }
            }
        }

        await outboxRepository.SaveChangesAsync(cancellationToken);
        await _logRepository.SaveChangesAsync(cancellationToken);

        return processed;
    }

    private string ExtractRecipient(OutboxMessage message)
    {
        try
        {
            return message.Channel switch
            {
                ECommunicationChannel.Email => JsonSerializer.Deserialize<EmailOutboxPayload>(message.Payload)?.To ?? "unknown",
                ECommunicationChannel.Sms => JsonSerializer.Deserialize<SmsOutboxPayload>(message.Payload)?.PhoneNumber ?? "unknown",
                ECommunicationChannel.Push => JsonSerializer.Deserialize<PushOutboxPayload>(message.Payload)?.DeviceToken ?? "unknown",
                _ => "unknown"
            };
        }
        catch
        {
            return "error-extracting";
        }
    }

    private Task<bool> DispatchMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        return message.Channel switch
        {
            ECommunicationChannel.Email => DispatchEmailAsync(message, cancellationToken),
            ECommunicationChannel.Sms => DispatchSmsAsync(message, cancellationToken),
            ECommunicationChannel.Push => DispatchPushAsync(message, cancellationToken),
            _ => throw new InvalidOperationException($"Unknown channel: {message.Channel}")
        };
    }

    private async Task<bool> DispatchEmailAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var email = JsonSerializer.Deserialize<EmailOutboxPayload>(message.Payload)
            ?? throw new InvalidOperationException("Invalid email payload.");

        return await emailSender.SendAsync(
            new Domain.Services.EmailMessage(email.To, email.Subject, email.HtmlBody, email.TextBody, email.From),
            cancellationToken);
    }

    private async Task<bool> DispatchSmsAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var sms = JsonSerializer.Deserialize<SmsOutboxPayload>(message.Payload)
            ?? throw new InvalidOperationException("Invalid SMS payload.");

        return await smsSender.SendAsync(
            new Domain.Services.SmsMessage(sms.PhoneNumber, sms.Body),
            cancellationToken);
    }

    private async Task<bool> DispatchPushAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var push = JsonSerializer.Deserialize<PushOutboxPayload>(message.Payload)
            ?? throw new InvalidOperationException("Invalid push payload.");

        return await pushSender.SendAsync(
            new Domain.Services.PushNotification(push.DeviceToken, push.Title, push.Body, push.Data),
            cancellationToken);
    }

    // Payload records (serialized into Outbox)
    private sealed record EmailOutboxPayload(string To, string Subject, string HtmlBody, string TextBody, string? From = null);
    private sealed record SmsOutboxPayload(string PhoneNumber, string Body);
    private sealed record PushOutboxPayload(string DeviceToken, string Title, string Body, IDictionary<string, string>? Data = null);
}
