using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OutboxMessage = MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage;

namespace MeAjudaAi.Modules.Communications.Application.Services.Outbox;

public sealed class OutboxProcessorService(
    IOutboxMessageRepository outboxRepository,
    [FromKeyedServices(ModuleKeys.Communications)] IUnitOfWork uow,
    IEmailSender emailSender,
    ISmsSender smsSender,
    IPushSender pushSender,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    IEmailTemplateQueries templateQueries,
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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return DispatchResult.Canceled();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Dispatch internal failure for outbox message {Id} ({Channel}).", message.Id, message.Channel);
            return DispatchResult.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            // Do not swallow unknown exceptions — log and rethrow so caller can decide how to handle them.
            logger.LogError(ex, "Unexpected error dispatching outbox message {Id} ({Channel}). Rethrowing.", message.Id, message.Channel);
            throw;
        }
    }

    protected override async Task OnSuccessAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var recipientRaw = ExtractRecipient(message);
        var recipientMasked = MaskRecipientForChannel(recipientRaw, message.Channel);
        CommunicationLog? log = null;

        try
        {
            log = CommunicationLog.CreateSuccess(
                correlationId: message.CorrelationId ?? $"outbox:{message.Id}",
                channel: message.Channel,
                recipient: recipientRaw,
                attemptCount: message.RetryCount + 1,
                outboxMessageId: message.Id,
                templateKey: ExtractTemplateKey(message));
            
            uow.GetRepository<CommunicationLog, Guid>().Add(log);
            await uow.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create success log for outbox message {Id} (CorrelationId: {CorrelationId}).", 
                message.Id, message.CorrelationId);
            if (log != null) uow.GetRepository<CommunicationLog, Guid>().Delete(log);
        }

        logger.LogInformation("Outbox message {Id} ({Channel}) sent to {Recipient}.", 
            message.Id, message.Channel, recipientMasked);
    }

    protected override async Task OnFailureAsync(OutboxMessage message, string? error, CancellationToken cancellationToken)
    {
        var recipientRaw = ExtractRecipient(message);
        var recipientMasked = MaskRecipientForChannel(recipientRaw, message.Channel);

        if (!message.HasRetriesLeft)
        {
            CommunicationLog? log = null;
            try
            {
                log = CommunicationLog.CreateFailure(
                    correlationId: message.CorrelationId ?? $"outbox:{message.Id}",
                    channel: message.Channel,
                    recipient: recipientRaw,
                    errorMessage: error ?? "Max retries reached.",
                    attemptCount: message.RetryCount,
                    outboxMessageId: message.Id,
                    templateKey: ExtractTemplateKey(message));
                
                uow.GetRepository<CommunicationLog, Guid>().Add(log);
                await uow.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create failure log for outbox message {Id} (CorrelationId: {CorrelationId}).", 
                    message.Id, message.CorrelationId);
                if (log != null) uow.GetRepository<CommunicationLog, Guid>().Delete(log);
            }
        }

        logger.LogWarning("Outbox message {Id} dispatch to {Channel} for {Recipient} failed. Attempt {Retry}/{Max}.",
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
        var email = serializer.Deserialize<EmailOutboxPayload>(message.Payload)
            ?? throw new InvalidOperationException("Invalid email payload.");

        string htmlBody;
        string textBody;

        if (!string.IsNullOrWhiteSpace(email.TemplateKey))
        {
            var template = await templateQueries.GetActiveByKeyAsync(email.TemplateKey, cancellationToken: cancellationToken);
            if (template != null)
            {
                htmlBody = RenderTemplate(template.HtmlBody, email.TemplateData);
                textBody = RenderTemplate(template.TextBody, email.TemplateData);
            }
            else
            {
                logger.LogWarning("Template {TemplateKey} not found for outbox message {Id}.", email.TemplateKey, message.Id);
                htmlBody = email.HtmlBody ?? (email.Body != null ? System.Net.WebUtility.HtmlEncode(email.Body) : string.Empty);
                textBody = email.TextBody ?? email.Body ?? string.Empty;
            }
        }
        else
        {
            htmlBody = email.HtmlBody ?? (email.Body != null ? System.Net.WebUtility.HtmlEncode(email.Body) : string.Empty);
            textBody = email.TextBody ?? email.Body ?? string.Empty;
        }

        return await emailSender.SendAsync(
            new EmailMessage(email.To, email.Subject, htmlBody, textBody, email.From),
            cancellationToken);
    }

    private static string RenderTemplate(string template, IDictionary<string, string>? data)
    {
        if (data == null || data.Count == 0) return template;
        
        var result = template;
        foreach (var entry in data)
        {
            result = result.Replace($"{{{{{entry.Key}}}}}", entry.Value);
        }
        return result;
    }

    private async Task<bool> DispatchSmsAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var sms = serializer.Deserialize<SmsOutboxPayload>(message.Payload)
            ?? throw new InvalidOperationException("Invalid SMS payload.");

        return await smsSender.SendAsync(
            new SmsMessage(sms.PhoneNumber, sms.Body),
            cancellationToken);
    }

    private async Task<bool> DispatchPushAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var push = serializer.Deserialize<PushOutboxPayload>(message.Payload)
            ?? throw new InvalidOperationException("Invalid push payload.");

        return await pushSender.SendAsync(
            new PushNotification(push.DeviceToken, push.Title, push.Body, push.Data),
            cancellationToken);
    }

    private static string MaskRecipientForChannel(string recipient, ECommunicationChannel channel) => channel switch
        {
            ECommunicationChannel.Email => PiiMaskingHelper.MaskEmail(recipient),
            ECommunicationChannel.Sms => PiiMaskingHelper.MaskPhoneNumber(recipient),
            ECommunicationChannel.Push => PiiMaskingHelper.MaskSensitiveData(recipient),
            _ => PiiMaskingHelper.MaskSensitiveData(recipient)
        };

    private string? ExtractTemplateKey(OutboxMessage message)
    {
        if (message.Channel != ECommunicationChannel.Email) return null;
        if (string.IsNullOrWhiteSpace(message.Payload)) return null;

        try
        {
            return serializer.Deserialize<EmailOutboxPayload>(message.Payload)?.TemplateKey;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private string ExtractRecipient(OutboxMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Payload))
            return "unknown";

        try
        {
            return message.Channel switch
            {
                ECommunicationChannel.Email => serializer.Deserialize<EmailOutboxPayload>(message.Payload)?.To ?? "unknown",
                ECommunicationChannel.Sms => serializer.Deserialize<SmsOutboxPayload>(message.Payload)?.PhoneNumber ?? "unknown",
                ECommunicationChannel.Push => serializer.Deserialize<PushOutboxPayload>(message.Payload)?.DeviceToken ?? "unknown",
                _ => "unknown"
            };
        }
        catch (Exception)
        {
            return "error-extracting";
        }
    }
}
