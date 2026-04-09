using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Contracts.Shared;
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
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        CancellationToken ct = default)
    {
        if (email == null) return Result<Guid>.Failure(Error.BadRequest("A mensagem de e-mail não pode ser nula."));
        if (string.IsNullOrWhiteSpace(email.To)) return Result<Guid>.Failure(Error.BadRequest("O e-mail do destinatário é obrigatório."));
        if (string.IsNullOrWhiteSpace(email.Subject)) return Result<Guid>.Failure(Error.BadRequest("O assunto do e-mail é obrigatório."));
        if (string.IsNullOrWhiteSpace(email.Body)) return Result<Guid>.Failure(Error.BadRequest("O corpo do e-mail é obrigatório."));
        
        if (!Enum.IsDefined(typeof(ECommunicationPriority), priority))
            return Result<Guid>.Failure(Error.BadRequest("Prioridade de comunicação inválida."));

        return await EnqueueOutboxAsync(ECommunicationChannel.Email, email, priority, ct);
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

    public async Task<Result<Guid>> SendSmsAsync(
        SmsMessageDto sms, 
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        CancellationToken ct = default)
    {
        if (sms == null) return Result<Guid>.Failure(Error.BadRequest("A mensagem SMS não pode ser nula."));
        if (string.IsNullOrWhiteSpace(sms.PhoneNumber)) return Result<Guid>.Failure(Error.BadRequest("O número de telefone é obrigatório."));
        if (string.IsNullOrWhiteSpace(sms.Message)) return Result<Guid>.Failure(Error.BadRequest("O corpo da mensagem SMS é obrigatório."));

        if (!Enum.IsDefined(typeof(ECommunicationPriority), priority))
            return Result<Guid>.Failure(Error.BadRequest("Prioridade de comunicação inválida."));

        return await EnqueueOutboxAsync(ECommunicationChannel.Sms, sms, priority, ct);
    }

    public async Task<Result<Guid>> SendPushAsync(
        PushMessageDto push, 
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        CancellationToken ct = default)
    {
        if (push == null) return Result<Guid>.Failure(Error.BadRequest("A notificação push não pode ser nula."));
        if (string.IsNullOrWhiteSpace(push.DeviceToken)) return Result<Guid>.Failure(Error.BadRequest("O token do dispositivo é obrigatório."));
        if (string.IsNullOrWhiteSpace(push.Title)) return Result<Guid>.Failure(Error.BadRequest("O título do push é obrigatório."));
        if (string.IsNullOrWhiteSpace(push.Body)) return Result<Guid>.Failure(Error.BadRequest("O corpo do push é obrigatório."));

        if (!Enum.IsDefined(typeof(ECommunicationPriority), priority))
            return Result<Guid>.Failure(Error.BadRequest("Prioridade de comunicação inválida."));

        return await EnqueueOutboxAsync(ECommunicationChannel.Push, push, priority, ct);
    }

    public async Task<Result<PagedResult<CommunicationLogDto>>> GetLogsAsync(
        CommunicationLogQuery query,
        CancellationToken ct = default)
    {
        if (query == null) return Result<PagedResult<CommunicationLogDto>>.Failure(Error.BadRequest("A consulta não pode ser nula."));
        if (query.PageNumber < 1) return Result<PagedResult<CommunicationLogDto>>.Failure(Error.BadRequest("O número da página deve ser pelo menos 1."));
        if (query.PageSize < 1 || query.PageSize > 100) return Result<PagedResult<CommunicationLogDto>>.Failure(Error.BadRequest("O tamanho da página deve estar entre 1 e 100."));

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

    private async Task<Result<Guid>> EnqueueOutboxAsync<TPayload>(
        ECommunicationChannel channel, 
        TPayload payload, 
        ECommunicationPriority priority, 
        CancellationToken ct)
    {
        var serializedPayload = JsonSerializer.Serialize(payload);
        var message = OutboxMessage.Create(
            channel,
            serializedPayload,
            priority);

        await outboxRepository.AddAsync(message, ct);
        await outboxRepository.SaveChangesAsync(ct);
        
        return Result<Guid>.Success(message.Id);
    }
}
