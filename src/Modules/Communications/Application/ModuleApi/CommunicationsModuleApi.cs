using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Utilities.Constants;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.ModuleApi;

/// <summary>
/// Implementação da API pública do módulo de comunicações.
/// </summary>
[MeAjudaAi.Contracts.Modules.ModuleApi(ModuleNames.Communications)]
internal sealed class CommunicationsModuleApi(
    IOutboxMessageRepository outboxRepository,
    IEmailTemplateRepository templateRepository,
    ICommunicationLogRepository logRepository)
    : ICommunicationsModuleApi
{
    private readonly IEmailTemplateRepository _templateRepository = templateRepository;

    public string ModuleName => ModuleNames.Communications;
    public string ApiVersion => "1.0";

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public async Task<Result<Guid>> SendEmailAsync(
        EmailMessageDto email,
        CommunicationPriority priority = CommunicationPriority.Normal,
        CancellationToken ct = default)
    {
        if (email == null) return Result<Guid>.Failure(Error.BadRequest("Email message cannot be null."));
        if (string.IsNullOrWhiteSpace(email.To)) return Result<Guid>.Failure(Error.BadRequest("Recipient email is required."));
        if (string.IsNullOrWhiteSpace(email.Subject)) return Result<Guid>.Failure(Error.BadRequest("Email subject is required."));
        if (string.IsNullOrWhiteSpace(email.Body)) return Result<Guid>.Failure(Error.BadRequest("Email body is required."));
        
        if (!Enum.IsDefined(typeof(CommunicationPriority), priority))
            return Result<Guid>.Failure(Error.BadRequest("Invalid communication priority."));

        var payload = JsonSerializer.Serialize(email);
        var message = OutboxMessage.Create(
            ECommunicationChannel.Email,
            payload,
            MapPriority(priority));

        await outboxRepository.AddAsync(message, ct);
        await outboxRepository.SaveChangesAsync(ct);
        
        return Result<Guid>.Success(message.Id);
    }

    public async Task<Result<IReadOnlyList<EmailTemplateDto>>> GetTemplatesAsync(CancellationToken ct = default)
    {
        var templates = await _templateRepository.GetAllAsync(ct);
        
        var dtos = templates.Select(x => new EmailTemplateDto(
            x.Id,
            x.TemplateKey,
            x.Subject,
            x.HtmlBody,
            x.TextBody,
            x.IsSystemTemplate,
            x.Language)).ToList();

        return Result<IReadOnlyList<EmailTemplateDto>>.Success(dtos);
    }

    public async Task<Result<Guid>> SendSmsAsync(SmsMessageDto sms, CancellationToken ct = default)
    {
        if (sms == null) return Result<Guid>.Failure(Error.BadRequest("SMS message cannot be null."));
        if (string.IsNullOrWhiteSpace(sms.PhoneNumber)) return Result<Guid>.Failure(Error.BadRequest("Phone number is required."));
        if (string.IsNullOrWhiteSpace(sms.Message)) return Result<Guid>.Failure(Error.BadRequest("SMS message body is required."));

        var payload = JsonSerializer.Serialize(sms);
        var message = OutboxMessage.Create(
            ECommunicationChannel.Sms,
            payload,
            ECommunicationPriority.Normal);

        await outboxRepository.AddAsync(message, ct);
        await outboxRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(message.Id);
    }

    public async Task<Result<Guid>> SendPushAsync(PushMessageDto push, CancellationToken ct = default)
    {
        if (push == null) return Result<Guid>.Failure(Error.BadRequest("Push notification cannot be null."));
        if (string.IsNullOrWhiteSpace(push.DeviceToken)) return Result<Guid>.Failure(Error.BadRequest("Device token is required."));
        if (string.IsNullOrWhiteSpace(push.Title)) return Result<Guid>.Failure(Error.BadRequest("Push title is required."));
        if (string.IsNullOrWhiteSpace(push.Body)) return Result<Guid>.Failure(Error.BadRequest("Push body is required."));

        var payload = JsonSerializer.Serialize(push);
        var message = OutboxMessage.Create(
            ECommunicationChannel.Push,
            payload,
            ECommunicationPriority.Normal);

        await outboxRepository.AddAsync(message, ct);
        await outboxRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(message.Id);
    }

    public async Task<Result<PagedResult<CommunicationLogDto>>> GetLogsAsync(
        CommunicationLogQuery query,
        CancellationToken ct = default)
    {
        if (query == null) return Result<PagedResult<CommunicationLogDto>>.Failure(Error.BadRequest("Query cannot be null."));
        if (query.PageNumber < 1) return Result<PagedResult<CommunicationLogDto>>.Failure(Error.BadRequest("Page number must be at least 1."));
        if (query.PageSize < 1 || query.PageSize > 100) return Result<PagedResult<CommunicationLogDto>>.Failure(Error.BadRequest("Page size must be between 1 and 100."));

        var (items, totalCount) = await logRepository.SearchAsync(
            query.CorrelationId,
            query.Channel,
            query.Recipient,
            query.IsSuccess,
            query.PageNumber,
            query.PageSize,
            ct);

        var dtos = items.Select(x => new CommunicationLogDto(
            x.Id,
            x.CorrelationId,
            x.Channel.ToString(),
            x.Recipient,
            x.TemplateKey,
            x.IsSuccess,
            x.ErrorMessage,
            x.AttemptCount,
            x.CreatedAt)).ToList();

        return Result<PagedResult<CommunicationLogDto>>.Success(new PagedResult<CommunicationLogDto>
        {
            Items = dtos,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalItems = totalCount
        });
    }

    private static ECommunicationPriority MapPriority(CommunicationPriority priority) => priority switch
    {
        CommunicationPriority.Low => ECommunicationPriority.Low,
        CommunicationPriority.High => ECommunicationPriority.High,
        _ => ECommunicationPriority.Normal
    };
}
