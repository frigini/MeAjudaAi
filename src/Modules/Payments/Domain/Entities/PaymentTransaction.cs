using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Payments.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    private PaymentTransaction() { }

    public PaymentTransaction(Guid subscriptionId, Money amount)
    {
        SubscriptionId = subscriptionId;
        Amount = amount;
        Status = EPaymentStatus.Pending;
        // CreatedAt já está em BaseEntity
    }

    public Guid SubscriptionId { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public EPaymentStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public void Settle(string externalTransactionId)
    {
        if (Status != EPaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot settle transaction in {Status} status.");

        if (string.IsNullOrWhiteSpace(externalTransactionId))
            throw new ArgumentException("ExternalTransactionId não pode ser vazio.", nameof(externalTransactionId));

        ExternalTransactionId = externalTransactionId;
        Status = EPaymentStatus.Succeeded;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Fail()
    {
        if (Status != EPaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot fail transaction in {Status} status.");

        Status = EPaymentStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }
}
