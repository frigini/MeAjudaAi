using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Contracts.Shared;

namespace MeAjudaAi.Shared.Database.Outbox;

/// <summary>
/// Representa uma mensagem genérica no Outbox para processamento assíncrono.
/// </summary>
public class OutboxMessage : BaseEntity
{
    protected OutboxMessage() { }

    /// <summary>
    /// Identificador único opcional para evitar duplicidade (Idempotência).
    /// </summary>
    public string? CorrelationId { get; protected set; }

    /// <summary>
    /// Tipo da mensagem (ex: nome do evento de integração, canal de comunicação).
    /// </summary>
    public string Type { get; protected set; } = string.Empty;

    /// <summary>
    /// Payload JSON serializado da mensagem.
    /// </summary>
    public string Payload { get; protected set; } = string.Empty;

    /// <summary>
    /// Status atual do processamento.
    /// </summary>
    public EOutboxMessageStatus Status { get; protected set; }

    /// <summary>
    /// Prioridade de entrega.
    /// </summary>
    public ECommunicationPriority Priority { get; protected set; }

    /// <summary>
    /// Contador de tentativas realizadas.
    /// </summary>
    public int RetryCount { get; protected set; }

    /// <summary>
    /// Número máximo de tentativas antes de marcar como falha definitiva.
    /// </summary>
    public int MaxRetries { get; protected set; }

    /// <summary>
    /// Momento agendado para processamento (null = processa imediatamente).
    /// </summary>
    public DateTime? ScheduledAt { get; protected set; }

    /// <summary>
    /// Momento em que foi enviada com sucesso.
    /// </summary>
    public DateTime? SentAt { get; protected set; }

    /// <summary>
    /// Mensagem de erro da última tentativa falhada.
    /// </summary>
    public string? ErrorMessage { get; protected set; }

    public static OutboxMessage Create(
        string type,
        string payload,
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        DateTime? scheduledAt = null,
        int maxRetries = 3,
        string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        if (maxRetries < 1)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "MaxRetries must be at least 1.");

        return new OutboxMessage
        {
            Type = type,
            Payload = payload,
            Status = EOutboxMessageStatus.Pending,
            Priority = priority,
            RetryCount = 0,
            MaxRetries = maxRetries,
            ScheduledAt = scheduledAt,
            CorrelationId = correlationId
        };
    }

    public bool IsReadyToProcess(DateTime utcNow)
        => Status == EOutboxMessageStatus.Pending
        && (ScheduledAt == null || ScheduledAt <= utcNow);

    public void MarkAsProcessing()
    {
        if (Status != EOutboxMessageStatus.Pending)
            return;

        Status = EOutboxMessageStatus.Processing;
        MarkAsUpdated();
    }

    public void ResetToPending()
    {
        if (Status != EOutboxMessageStatus.Processing)
            return;

        Status = EOutboxMessageStatus.Pending;
        MarkAsUpdated();
    }

    public void MarkAsSent(DateTime utcNow)
    {
        if (Status is EOutboxMessageStatus.Sent or EOutboxMessageStatus.Failed)
            return;

        Status = EOutboxMessageStatus.Sent;
        SentAt = utcNow;
        ErrorMessage = null;
        MarkAsUpdated();
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (Status is EOutboxMessageStatus.Sent or EOutboxMessageStatus.Failed)
            return;

        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        RetryCount++;
        ErrorMessage = errorMessage;

        if (RetryCount >= MaxRetries)
        {
            Status = EOutboxMessageStatus.Failed;
        }
        else
        {
            Status = EOutboxMessageStatus.Pending;
        }
        
        MarkAsUpdated();
    }

    public bool HasRetriesLeft => RetryCount < MaxRetries;
}
