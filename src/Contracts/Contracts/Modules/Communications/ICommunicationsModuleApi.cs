using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;

namespace MeAjudaAi.Contracts.Modules.Communications;

/// <summary>
/// Prioridade de entrega de uma comunicação.
/// </summary>
public enum CommunicationPriority
{
    Low = 0,
    Normal = 1,
    High = 2
}

/// <summary>
/// API pública para o módulo de comunicações (E-mail, SMS, Push).
/// </summary>
public interface ICommunicationsModuleApi : IModuleApi
{
    /// <summary>
    /// Envia uma mensagem de e-mail (enfileira no outbox).
    /// </summary>
    /// <param name="email">DTO com dados do e-mail</param>
    /// <param name="priority">Prioridade de envio</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>ID da mensagem no outbox</returns>
    Task<Result<Guid>> SendEmailAsync(
        EmailMessageDto email,
        CommunicationPriority priority = CommunicationPriority.Normal,
        CancellationToken ct = default);

    /// <summary>
    /// Obtém todos os templates de e-mail disponíveis.
    /// </summary>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Lista de templates</returns>
    Task<Result<IReadOnlyList<EmailTemplateDto>>> GetTemplatesAsync(CancellationToken ct = default);

    /// <summary>
    /// Envia uma mensagem SMS (enfileira no outbox).
    /// </summary>
    /// <param name="sms">DTO com dados do SMS</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>ID da mensagem no outbox</returns>
    Task<Result<Guid>> SendSmsAsync(SmsMessageDto sms, CancellationToken ct = default);

    /// <summary>
    /// Envia uma notificação push (enfileira no outbox).
    /// </summary>
    /// <param name="push">DTO com dados do push</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>ID da mensagem no outbox</returns>
    Task<Result<Guid>> SendPushAsync(PushMessageDto push, CancellationToken ct = default);

    /// <summary>
    /// Obtém logs de comunicação paginados (Idempotency check via CorrelationId).
    /// </summary>
    /// <param name="query">Critérios de busca</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Resultado paginado de logs</returns>
    Task<Result<PagedResult<CommunicationLogDto>>> GetLogsAsync(
        CommunicationLogQuery query,
        CancellationToken ct = default);
}
