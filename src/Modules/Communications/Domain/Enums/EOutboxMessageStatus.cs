namespace MeAjudaAi.Modules.Communications.Domain.Enums;

/// <summary>
/// Status de processamento de uma mensagem no Outbox.
/// </summary>
public enum EOutboxMessageStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}
