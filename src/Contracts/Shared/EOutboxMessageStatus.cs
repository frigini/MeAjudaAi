namespace MeAjudaAi.Contracts.Shared;

/// <summary>
/// Status de processamento de uma mensagem no Outbox.
/// </summary>
public enum EOutboxMessageStatus
{
    Pending = 0,
    Processing = 1,
    Sent = 2,
    Failed = 3
}
