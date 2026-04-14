using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Payments.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    private PaymentTransaction() { }

    public PaymentTransaction(Guid subscriptionId, Money amount)
    {
        Id = Guid.NewGuid();
        SubscriptionId = subscriptionId;
        Amount = amount;
        Status = EPaymentStatus.Pending;
        // CreatedAt is already in BaseEntity
    }

    public Guid SubscriptionId { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public EPaymentStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public void Settle(string externalTransactionId)
    {
        ExternalTransactionId = externalTransactionId;
        Status = EPaymentStatus.Succeeded;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Fail()
    {
        Status = EPaymentStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }
}
