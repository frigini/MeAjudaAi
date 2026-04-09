using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Contracts.Shared;

namespace MeAjudaAi.Modules.Communications.Domain.Entities;

/// <summary>
/// Representa uma mensagem no Outbox aguardando processamento assíncrono.
/// </summary>
/// <remarks>
/// O Outbox Pattern garante entrega confiável de mensagens:
/// a mensagem é persistida na mesma transação do evento de domínio,
/// e um worker processa o Outbox de forma independente.
/// Suporta agendamento futuro via <see cref="ScheduledAt"/> e retries automáticos.
/// </remarks>
public sealed class OutboxMessage : BaseEntity
{
    private OutboxMessage() { }

    /// <summary>
    /// Identificador único opcional para evitar duplicidade (Idempotência).
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    /// Canal de comunicação: Email, Sms ou Push.
    /// </summary>
    public ECommunicationChannel Channel { get; private set; }

    /// <summary>
    /// Payload JSON serializado da mensagem.
    /// </summary>
    public string Payload { get; private set; } = string.Empty;

    /// <summary>
    /// Status atual do processamento.
    /// </summary>
    public EOutboxMessageStatus Status { get; private set; }

    /// <summary>
    /// Prioridade de entrega. Mensagens de alta prioridade são processadas primeiro.
    /// </summary>
    public ECommunicationPriority Priority { get; private set; }

    /// <summary>
    /// Contador de tentativas realizadas.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Número máximo de tentativas antes de marcar como falha definitiva.
    /// </summary>
    public int MaxRetries { get; private set; }

    /// <summary>
    /// Momento agendado para processamento (null = processa imediatamente).
    /// </summary>
    public DateTime? ScheduledAt { get; private set; }

    /// <summary>
    /// Momento em que foi enviada com sucesso.
    /// </summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>
    /// Mensagem de erro da última tentativa falhada.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Cria uma nova mensagem no Outbox.
    /// </summary>
    public static OutboxMessage Create(
        ECommunicationChannel channel,
        string payload,
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        DateTime? scheduledAt = null,
        int maxRetries = 3,
        string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        if (maxRetries < 1)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "MaxRetries must be at least 1.");

        return new OutboxMessage
        {
            Channel = channel,
            Payload = payload,
            Status = EOutboxMessageStatus.Pending,
            Priority = priority,
            RetryCount = 0,
            MaxRetries = maxRetries,
            ScheduledAt = scheduledAt,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Verifica se a mensagem está pronta para processamento (respeitando agendamento).
    /// </summary>
    public bool IsReadyToProcess(DateTime utcNow)
        => Status == EOutboxMessageStatus.Pending
        && (ScheduledAt == null || ScheduledAt <= utcNow);

    /// <summary>
    /// Marca a mensagem como em processamento para evitar que outros workers a capturem.
    /// </summary>
    public void MarkAsProcessing()
    {
        if (Status != EOutboxMessageStatus.Pending)
            return;

        Status = EOutboxMessageStatus.Processing;
        MarkAsUpdated();
    }

    /// <summary>
    /// Reseta uma mensagem travada em processamento de volta para pendente.
    /// </summary>
    public void ResetToPending()
    {
        if (Status != EOutboxMessageStatus.Processing)
            return;

        Status = EOutboxMessageStatus.Pending;
        MarkAsUpdated();
    }

    /// <summary>
    /// Registra envio bem-sucedido.
    /// </summary>
    public void MarkAsSent(DateTime utcNow)
    {
        if (Status is EOutboxMessageStatus.Sent or EOutboxMessageStatus.Failed)
            return;

        Status = EOutboxMessageStatus.Sent;
        SentAt = utcNow;
        ErrorMessage = null;
        MarkAsUpdated();
    }

    /// <summary>
    /// Registra falha em uma tentativa. Se exceder MaxRetries, marca como falha definitiva.
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        if (Status is EOutboxMessageStatus.Sent or EOutboxMessageStatus.Failed)
            return;

        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        RetryCount++;
        ErrorMessage = errorMessage;
        MarkAsUpdated();

        if (RetryCount >= MaxRetries)
        {
            Status = EOutboxMessageStatus.Failed;
        }
        else
        {
            // Reseta para Pending para permitir que seja capturada novamente pelo polling
            Status = EOutboxMessageStatus.Pending;
        }
        
        MarkAsUpdated();
    }

    /// <summary>
    /// Indica se ainda há tentativas disponíveis.
    /// </summary>
    public bool HasRetriesLeft => RetryCount < MaxRetries;
}
