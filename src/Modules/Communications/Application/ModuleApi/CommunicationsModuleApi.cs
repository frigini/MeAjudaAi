using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.ModuleApi;

[ModuleApi(ModuleNames.Communications)]
public sealed class CommunicationsModuleApi(
    IOutboxMessageRepository outboxRepository,
    IEmailTemplateQueries templateQueries,
    ICommunicationLogQueries logQueries,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    IServiceProvider serviceProvider,
    ILogger<CommunicationsModuleApi> logger)
    : ICommunicationsModuleApi
{
    private readonly IEmailTemplateQueries _templateQueries = templateQueries;

    public string ModuleName => ModuleNames.Communications;
    public string ApiVersion => "1.0";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking Communications module availability");

            // Verifica health checks registrados do sistema
            var healthCheckService = serviceProvider.GetService<HealthCheckService>();
            if (healthCheckService != null)
            {
                var healthReport = await healthCheckService.CheckHealthAsync(
                    check => check.Tags.Contains("communications") || check.Tags.Contains("database"),
                    cancellationToken);

                if (healthReport.Status == HealthStatus.Unhealthy || healthReport.Status == HealthStatus.Degraded)
                {
                    logger.LogWarning("Communications module unavailable due to health check failures: {FailedChecks}",
                        string.Join(", ", healthReport.Entries.Where(e => e.Value.Status == HealthStatus.Unhealthy || e.Value.Status == HealthStatus.Degraded).Select(e => e.Key)));
                    return false;
                }

            }

            // Testa funcionalidade básica
            var canExecuteBasicOperations = await CanExecuteBasicOperationsAsync(cancellationToken);
            if (!canExecuteBasicOperations)
            {
                logger.LogWarning("Communications module unavailable - basic operations test failed");
                return false;
            }

            logger.LogDebug("Communications module is available and healthy");
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogDebug("Communications module availability check canceled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Communications module availability");
            return false;
        }
    }

    private async Task<bool> CanExecuteBasicOperationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Teste básico: verificar se existem templates (operação leve de banco)
            var templates = await _templateQueries.GetAllAsync(cancellationToken);
            return templates.Count > 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Basic operations test failed for Communications module");
            return false;
        }
    }

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
        var templates = await _templateQueries.GetAllAsync(ct);
        
        var dtos = templates.Select(x => new EmailTemplateDto(
            x.Id,
            x.TemplateKey,
            x.Subject,
            x.HtmlBody,
            x.TextBody,
            x.IsSystemTemplate,
            x.Language,
            x.Version)).ToList();

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
        if (query.PageNumber < 1 || query.PageSize < 1 || query.PageSize > 100) return Result<PagedResult<CommunicationLogDto>>.Failure(Error.BadRequest("Parâmetros de paginação inválidos."));
        
        var (items, totalCount) = await logQueries.SearchAsync(query, ct);

        var dtos = (items ?? new List<CommunicationLog>()).Select(x => new CommunicationLogDto(
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
