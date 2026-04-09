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
        ArgumentNullException.ThrowIfNull(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(email.To);
        ArgumentException.ThrowIfNullOrWhiteSpace(email.Subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(email.Body);

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
        ArgumentNullException.ThrowIfNull(sms);
        ArgumentException.ThrowIfNullOrWhiteSpace(sms.PhoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(sms.Message);

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
        ArgumentNullException.ThrowIfNull(push);
        ArgumentException.ThrowIfNullOrWhiteSpace(push.DeviceToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(push.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(push.Body);

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
