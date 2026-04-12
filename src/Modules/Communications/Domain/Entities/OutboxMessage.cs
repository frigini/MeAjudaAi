using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Contracts.Shared;

namespace MeAjudaAi.Modules.Communications.Domain.Entities;

/// <summary>
/// Representa uma mensagem no Outbox específica do módulo de comunicações.
/// Herda da base genérica para aproveitar lógica de processamento e retries.
/// </summary>
public sealed class OutboxMessage : MeAjudaAi.Shared.Database.Outbox.OutboxMessage
{
    private OutboxMessage() { }

    /// <summary>
    /// Canal de comunicação: Email, Sms ou Push.
    /// No banco, este campo é específico deste módulo.
    /// </summary>
    public ECommunicationChannel Channel { get; private set; }

    /// <summary>
    /// Cria uma nova mensagem no Outbox de comunicações.
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
            Type = channel.ToString(), // Mapeia o canal para o campo Type da base
            Payload = payload,
            Status = EOutboxMessageStatus.Pending,
            Priority = priority,
            RetryCount = 0,
            MaxRetries = maxRetries,
            ScheduledAt = scheduledAt,
            CorrelationId = correlationId
        };
    }
}
