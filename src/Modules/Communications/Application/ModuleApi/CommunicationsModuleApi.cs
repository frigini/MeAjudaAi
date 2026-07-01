using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Modules.Communications.Application.DTOs;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace MeAjudaAi.Modules.Communications.Application.ModuleApi;

[ModuleApi(ModuleNames.Communications)]
public sealed class CommunicationsModuleApi(
    IOutboxMessageRepository outboxRepository,
    IEmailTemplateQueries templateQueries,
    ICommunicationLogQueries logQueries,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    IStringLocalizer<Strings> localizer)
    : ICommunicationsModuleApi
{
    private readonly IEmailTemplateQueries _templateQueries = templateQueries;
    private readonly IStringLocalizer<Strings> _localizer = localizer;

    public string ModuleName => ModuleNames.Communications;
    public string ApiVersion => "1.0";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await _templateQueries.CanConnectAsync(cancellationToken);
    }

    public async Task<Result<Guid>> SendEmailAsync(
        EmailMessageDto email,
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        CancellationToken ct = default)
    {
        if (email == null) return Result<Guid>.Failure(Error.BadRequest(_localizer["EmailMessageCannotBeNull"]));
        if (string.IsNullOrWhiteSpace(email.To)) return Result<Guid>.Failure(Error.BadRequest(_localizer["RecipientEmailRequired"]));
        if (string.IsNullOrWhiteSpace(email.Subject)) return Result<Guid>.Failure(Error.BadRequest(_localizer["EmailSubjectRequired"]));
        
        if (!Enum.IsDefined(typeof(ECommunicationPriority), priority))
            return Result<Guid>.Failure(Error.BadRequest(_localizer["InvalidCommunicationPriority"]));

        EmailOutboxPayload payload;

        if (!string.IsNullOrWhiteSpace(email.TemplateKey))
        {
            payload = EmailOutboxPayload.Create(
                to: email.To,
                subject: email.Subject,
                templateKey: email.TemplateKey,
                templateData: email.TemplateData?.AsReadOnly());
        }
        else
        {
            if (string.IsNullOrWhiteSpace(email.Body))
                return Result<Guid>.Failure(Error.BadRequest(_localizer["EmailBodyRequired"]));

            payload = email.IsHtml
                ? EmailOutboxPayload.Create(to: email.To, subject: email.Subject, htmlBody: email.Body)
                : EmailOutboxPayload.Create(to: email.To, subject: email.Subject, textBody: email.Body);
        }

        return await EnqueueOutboxAsync(ECommunicationChannel.Email, payload, priority, ct);
    }

    public async Task<Result<IReadOnlyList<EmailTemplateDto>>> GetTemplatesAsync(CancellationToken ct = default)
    {
        var templates = await _templateQueries.GetAllAsync(ct);
        
        var dtos = templates.Select(x => new EmailTemplateDto(
            x.Id,
            x.TemplateKey,
            x.Subject,
            x.HtmlBody,
            x.TextBody,
            x.IsActive,
            x.IsSystemTemplate,
            x.Language,
            x.Version,
            x.OverrideKey)).ToList();

        return Result<IReadOnlyList<EmailTemplateDto>>.Success(dtos);
    }

    public async Task<Result<Guid>> SendSmsAsync(
        SmsMessageDto sms, 
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        CancellationToken ct = default)
    {
        if (sms == null) return Result<Guid>.Failure(Error.BadRequest(_localizer["SmsMessageCannotBeNull"]));
        if (string.IsNullOrWhiteSpace(sms.PhoneNumber)) return Result<Guid>.Failure(Error.BadRequest(_localizer["PhoneNumberRequired"]));
        if (string.IsNullOrWhiteSpace(sms.Message)) return Result<Guid>.Failure(Error.BadRequest(_localizer["SmsBodyRequired"]));

        if (!Enum.IsDefined(typeof(ECommunicationPriority), priority))
            return Result<Guid>.Failure(Error.BadRequest(_localizer["InvalidCommunicationPriority"]));

        var payload = new SmsOutboxPayload(sms.PhoneNumber, sms.Message);
        return await EnqueueOutboxAsync(ECommunicationChannel.Sms, payload, priority, ct);
    }

    public async Task<Result<Guid>> SendPushAsync(
        PushMessageDto push, 
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        CancellationToken ct = default)
    {
        if (push == null) return Result<Guid>.Failure(Error.BadRequest(_localizer["PushNotificationCannotBeNull"]));
        if (string.IsNullOrWhiteSpace(push.DeviceToken)) return Result<Guid>.Failure(Error.BadRequest(_localizer["DeviceTokenRequired"]));
        if (string.IsNullOrWhiteSpace(push.Title)) return Result<Guid>.Failure(Error.BadRequest(_localizer["PushTitleRequired"]));
        if (string.IsNullOrWhiteSpace(push.Body)) return Result<Guid>.Failure(Error.BadRequest(_localizer["PushBodyRequired"]));

        if (!Enum.IsDefined(typeof(ECommunicationPriority), priority))
            return Result<Guid>.Failure(Error.BadRequest(_localizer["InvalidCommunicationPriority"]));

        var payload = new PushOutboxPayload(push.DeviceToken, push.Title, push.Body, push.ExtraData);
        return await EnqueueOutboxAsync(ECommunicationChannel.Push, payload, priority, ct);
    }

    public async Task<Result<PagedResult<CommunicationLogDto>>> GetLogsAsync(
        CommunicationLogQuery query,
        CancellationToken ct = default)
    {
        if (query == null) return Result<PagedResult<CommunicationLogDto>>.Failure(Error.BadRequest(_localizer["QueryCannotBeNull"]));
        if (query.PageNumber < 1 || query.PageSize < 1 || query.PageSize > 100) return Result<PagedResult<CommunicationLogDto>>.Failure(Error.BadRequest(_localizer["InvalidPaginationParameters"]));
        
        var (items, totalCount) = await logQueries.SearchAsync(query, ct);

        var dtos = (items ?? new List<CommunicationLog>()).Select(x => new CommunicationLogDto(
            x.Id,
            x.CorrelationId,
            x.Channel.ToDescription(),
            x.Recipient,
            x.TemplateKey,
            x.IsSuccess,
            x.ErrorMessage,
            x.AttemptCount,
            x.CreatedAt,
            x.OutboxMessageId)).ToList();

        return Result<PagedResult<CommunicationLogDto>>.Success(new PagedResult<CommunicationLogDto>
        {
            Items = dtos,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalItems = totalCount
        });
    }

    private async Task<Result<Guid>> EnqueueOutboxAsync<TPayload>(
        ECommunicationChannel channel, 
        TPayload payload, 
        ECommunicationPriority priority, 
        CancellationToken ct)
    {
        var payloadData = serializer.Serialize(payload);
        var envelope = new MessageEnvelope(1, payloadData);
        var serializedPayload = serializer.Serialize(envelope);
        
        var message = OutboxMessage.Create(
            channel,
            serializedPayload,
            maxRetries: 3,
            priority: priority);

        await outboxRepository.AddAsync(message, ct);
        await outboxRepository.SaveChangesAsync(ct);
        
        return Result<Guid>.Success(message.Id);
    }
}
