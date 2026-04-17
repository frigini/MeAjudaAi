using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Payments.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    private PaymentTransaction() { }

    public PaymentTransaction(Guid subscriptionId, Money amount)
    {
        if (subscriptionId == Guid.Empty)
            throw new ArgumentException("SubscriptionId cannot be empty.", nameof(subscriptionId));

        if (amount == null)
            throw new ArgumentNullException(nameof(amount), "Amount cannot be null.");

        if (amount.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        SubscriptionId = subscriptionId;
        Amount = amount;
        Status = EPaymentStatus.Pending;
    }

    public Guid SubscriptionId { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public EPaymentStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    public void Settle(string externalTransactionId)
    {
        if (string.IsNullOrWhiteSpace(externalTransactionId))
            throw new ArgumentException("ExternalTransactionId cannot be empty.", nameof(externalTransactionId));

        if (Status != EPaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot settle transaction in {Status} status.");

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

    public void Refund()
    {
        if (Status != EPaymentStatus.Succeeded)
            throw new InvalidOperationException($"Cannot refund transaction in {Status} status. Only Succeeded transactions can be refunded.");

        if (Status == EPaymentStatus.Refunded) return;

        Status = EPaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }
}
