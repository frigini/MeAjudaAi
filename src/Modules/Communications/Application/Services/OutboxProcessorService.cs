using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Contracts.Shared;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using OutboxMessage = MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage;

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
/// Implementação do processador de Outbox específica para comunicações.
/// Estende a base genérica para aproveitar lógica de polling e retries.
/// </summary>
public sealed class OutboxProcessorService(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogRepository logRepository,
    IEmailSender emailSender,
    ISmsSender smsSender,
    IPushSender pushSender,
    ILogger<OutboxProcessorService> logger) 
    : OutboxProcessorBase<OutboxMessage>(outboxRepository, logger), IOutboxProcessorService
{
    protected override async Task<DispatchResult> DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var success = await DispatchInternalAsync(message, cancellationToken);
            return success 
                ? DispatchResult.Success() 
                : DispatchResult.Failure("Dispatch service returned false.");
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation to allow graceful worker shutdown without marking as failed
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error dispatching outbox message {Id} ({Channel}).", message.Id, message.Channel);
            return DispatchResult.Failure(ex.Message);
        }
    }

    protected override async Task OnSuccessAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var recipientRaw = ExtractRecipient(message);
        var recipientMasked = MaskRecipientForChannel(recipientRaw, message.Channel);

        var log = CommunicationLog.CreateSuccess(
            correlationId: message.CorrelationId ?? $"outbox:{message.Id}",
            channel: message.Channel,
            recipient: recipientRaw,
            attemptCount: message.RetryCount + 1,
            outboxMessageId: message.Id,
            templateKey: ExtractTemplateKey(message));
        
        await logRepository.AddAsync(log, cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Outbox message {Id} ({Channel}) sent to {Recipient}.", 
            message.Id, message.Channel, recipientMasked);
    }

    protected override async Task OnFailureAsync(OutboxMessage message, string? error, CancellationToken cancellationToken)
    {
        var recipientRaw = ExtractRecipient(message);
        var recipientMasked = MaskRecipientForChannel(recipientRaw, message.Channel);

        if (!message.HasRetriesLeft)
        {
            var log = CommunicationLog.CreateFailure(
                correlationId: message.CorrelationId ?? $"outbox:{message.Id}",
                channel: message.Channel,
                recipient: recipientRaw,
                errorMessage: error ?? "Max retries reached.",
                attemptCount: message.RetryCount,
                outboxMessageId: message.Id,
                templateKey: ExtractTemplateKey(message));
            
            await logRepository.AddAsync(log, cancellationToken);
            await logRepository.SaveChangesAsync(cancellationToken);
        }

        logger.LogWarning("Outbox message {Id} dispatch failed to {Recipient}. Retry {Retry}/{Max}.",
            message.Id, message.Channel, recipientMasked, message.RetryCount, message.MaxRetries);
    }

    private async Task<bool> DispatchInternalAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        return message.Channel switch
        {
            ECommunicationChannel.Email => await DispatchEmailAsync(message, cancellationToken),
            ECommunicationChannel.Sms => await DispatchSmsAsync(message, cancellationToken),
            ECommunicationChannel.Push => await DispatchPushAsync(message, cancellationToken),
            _ => throw new InvalidOperationException($"Unknown channel: {message.Channel}")
        };
    }

    private async Task<bool> DispatchEmailAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var email = JsonSerializer.Deserialize<EmailOutboxPayload>(message.Payload)
            ?? throw new InvalidOperationException("Invalid email payload.");

        var htmlBody = email.HtmlBody ?? (email.Body != null ? System.Net.WebUtility.HtmlEncode(email.Body) : string.Empty);
        var textBody = email.TextBody ?? email.Body ?? string.Empty;

        return await emailSender.SendAsync(
            new Domain.Services.EmailMessage(email.To, email.Subject, htmlBody, textBody, email.From),
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

    private string MaskRecipientForChannel(string recipient, ECommunicationChannel channel)
    {
        return channel switch
        {
            ECommunicationChannel.Email => PiiMaskingHelper.MaskEmail(recipient),
            ECommunicationChannel.Sms => PiiMaskingHelper.MaskPhoneNumber(recipient),
            ECommunicationChannel.Push => PiiMaskingHelper.MaskSensitiveData(recipient),
            _ => PiiMaskingHelper.MaskSensitiveData(recipient)
        };
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

    private sealed record EmailOutboxPayload(string To, string Subject, string? HtmlBody = null, string? TextBody = null, string? Body = null, string? From = null, string? TemplateKey = null);
    private sealed record SmsOutboxPayload(string PhoneNumber, string Body);
    private sealed record PushOutboxPayload(string DeviceToken, string Title, string Body, IDictionary<string, string>? Data = null);
}
