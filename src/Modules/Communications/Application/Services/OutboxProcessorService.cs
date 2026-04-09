using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Shared.Utilities;
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

            bool? dispatchSuccess = null;
            string? errorMessage = null;

            // 1. Despacho (Efeito Externo)
            try
            {
                dispatchSuccess = await DispatchMessageAsync(message, cancellationToken);
                if (dispatchSuccess == false)
                {
                    errorMessage = "Dispatch service returned false.";
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                logger.LogError(ex, "Error dispatching outbox message {Id} ({Channel}).", message.Id, message.Channel);
            }

            // 2. Persistência do Resultado (Estado Local)
            try
            {
                if (dispatchSuccess == true)
                {
                    message.MarkAsSent(DateTime.UtcNow);
                    
                    var log = CommunicationLog.CreateSuccess(
                        correlationId: message.CorrelationId ?? $"outbox:{message.Id}",
                        channel: message.Channel,
                        recipient: ExtractRecipient(message),
                        attemptCount: message.RetryCount,
                        outboxMessageId: message.Id,
                        templateKey: ExtractTemplateKey(message));
                    
                    await _logRepository.AddAsync(log, cancellationToken);
                    
                    // Persistência imediata para evitar duplicidade em caso de falha subsequente
                    await outboxRepository.SaveChangesAsync(cancellationToken);
                    await _logRepository.SaveChangesAsync(cancellationToken);

                    processed++;
                    logger.LogInformation("Outbox message {Id} ({Channel}) sent to {Recipient}.", 
                        message.Id, message.Channel, PiiMaskingHelper.MaskEmail(log.Recipient));
                }
                else
                {
                    // Falha no despacho ou retorno false
                    message.MarkAsFailed(errorMessage ?? "Unknown dispatch failure.");
                    
                    if (!message.HasRetriesLeft)
                    {
                        var log = CommunicationLog.CreateFailure(
                            correlationId: message.CorrelationId ?? $"outbox:{message.Id}",
                            channel: message.Channel,
                            recipient: ExtractRecipient(message),
                            errorMessage: errorMessage ?? "Max retries reached.",
                            attemptCount: message.RetryCount,
                            outboxMessageId: message.Id,
                            templateKey: ExtractTemplateKey(message));
                        await _logRepository.AddAsync(log, cancellationToken);
                        await _logRepository.SaveChangesAsync(cancellationToken);
                    }

                    await outboxRepository.SaveChangesAsync(cancellationToken);
                    logger.LogWarning("Outbox message {Id} dispatch failed. Retry {Retry}/{Max}.",
                        message.Id, message.RetryCount, message.MaxRetries);
                }
            }
            catch (Exception persistEx)
            {
                logger.LogCritical(persistEx, "CRITICAL: Failed to persist outbox state for message {Id}. This may cause duplicate delivery on next run.", message.Id);
                // Se falhou a persistência, não tentamos marcar como falha novamente (evita loop infinito de erros)
            }
        }

        return processed;
    }

    private string? ExtractTemplateKey(OutboxMessage message)
    {
        if (message.Channel != ECommunicationChannel.Email) return null;
        try
        {
            return JsonSerializer.Deserialize<EmailOutboxPayload>(message.Payload)?.TemplateKey;
        }
        catch
        {
            return null;
        }
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

    // Registros de payload (serializados no Outbox)
    private sealed record EmailOutboxPayload(string To, string Subject, string HtmlBody, string TextBody, string? From = null, string? TemplateKey = null);
    private sealed record SmsOutboxPayload(string PhoneNumber, string Body);
    private sealed record PushOutboxPayload(string DeviceToken, string Title, string Body, IDictionary<string, string>? Data = null);
}
