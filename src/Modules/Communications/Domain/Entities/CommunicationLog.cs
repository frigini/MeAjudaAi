using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Communications.Domain.Entities;

/// <summary>
/// Registro de auditoria de uma comunicação entregue (ou com falha definitiva).
/// </summary>
/// <remarks>
/// Utilizado para rastrear o histórico de comunicações enviadas, com suporte
/// a idempotência via <see cref="CorrelationId"/>. Antes de criar um novo registro,
/// o sistema verifica se já existe um com o mesmo CorrelationId para evitar duplicatas.
/// </remarks>
public sealed class CommunicationLog : BaseEntity
{
    private CommunicationLog() { }

    /// <summary>
    /// ID de correlação para garantir idempotência. Ex: "user_registered:{userId}".
    /// </summary>
    public string CorrelationId { get; private set; } = string.Empty;

    /// <summary>
    /// Canal pelo qual a comunicação foi enviada.
    /// </summary>
    public ECommunicationChannel Channel { get; private set; }

    /// <summary>
    /// Destinatário da comunicação (e-mail, número de telefone, etc.).
    /// </summary>
    public string Recipient { get; private set; } = string.Empty;

    /// <summary>
    /// Template key utilizado (quando aplicável).
    /// </summary>
    public string? TemplateKey { get; private set; }

    /// <summary>
    /// Indica se foi entregue com sucesso.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Mensagem de erro (quando IsSuccess = false).
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Número de tentativas realizadas até a entrega (ou falha definitiva).
    /// </summary>
    public int AttemptCount { get; private set; }

    /// <summary>
    /// ID da mensagem no Outbox (rastreabilidade).
    /// </summary>
    public Guid? OutboxMessageId { get; private set; }

    /// <summary>
    /// Cria um log de comunicação bem-sucedida.
    /// </summary>
    public static CommunicationLog CreateSuccess(
        string correlationId,
        ECommunicationChannel channel,
        string recipient,
        int attemptCount,
        Guid? outboxMessageId = null,
        string? templateKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentOutOfRangeException.ThrowIfNegative(attemptCount);

        return new CommunicationLog
        {
            CorrelationId = correlationId,
            Channel = channel,
            Recipient = recipient,
            TemplateKey = templateKey,
            IsSuccess = true,
            AttemptCount = attemptCount,
            OutboxMessageId = outboxMessageId
        };
    }

    /// <summary>
    /// Cria um log de comunicação com falha.
    /// </summary>
    public static CommunicationLog CreateFailure(
        string correlationId,
        ECommunicationChannel channel,
        string recipient,
        string errorMessage,
        int attemptCount,
        Guid? outboxMessageId = null,
        string? templateKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        ArgumentOutOfRangeException.ThrowIfNegative(attemptCount);

        return new CommunicationLog
        {
            CorrelationId = correlationId,
            Channel = channel,
            Recipient = recipient,
            TemplateKey = templateKey,
            IsSuccess = false,
            ErrorMessage = errorMessage,
            AttemptCount = attemptCount,
            OutboxMessageId = outboxMessageId
        };
    }
}
