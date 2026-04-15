using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Payments.Domain.Entities;

/// <summary>
/// Representa uma assinatura de serviço de um provider.
/// </summary>
/// <remarks>
/// CreatedAt está definido em BaseEntity.
/// </remarks>
public class Subscription : AggregateRoot<Guid>
{
    private Subscription() { }

    /// <summary>
    /// Cria uma nova assinatura para um provider.
    /// </summary>
    /// <param name="providerId">Identificador único do provider.</param>
    /// <param name="planId">Identificador do plano de assinatura.</param>
    /// <param name="amount">Valor monetário da assinatura.</param>
    public Subscription(Guid providerId, string planId, Money amount)
    {
        if (providerId == Guid.Empty)
            throw new ArgumentException("ProviderId cannot be empty.", nameof(providerId));

        if (string.IsNullOrWhiteSpace(planId))
            throw new ArgumentNullException(nameof(planId), "PlanId cannot be null or empty.");

        if (amount == null)
            throw new ArgumentNullException(nameof(amount), "Amount cannot be null.");

        if (amount.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        ProviderId = providerId;
        PlanId = planId;
        Amount = amount;
        Status = ESubscriptionStatus.Pending;
    }

    public Guid ProviderId { get; private set; }
    public string PlanId { get; private set; } = null!;
    public string? ExternalSubscriptionId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public ESubscriptionStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public void Activate(string externalSubscriptionId, DateTime startedAt, DateTime? expiresAt)
    {
        if (Status == ESubscriptionStatus.Active) return;
        if (Status == ESubscriptionStatus.Canceled) return;
        if (Status == ESubscriptionStatus.Expired) return;

        ExternalSubscriptionId = externalSubscriptionId;
        StartedAt = startedAt;
        ExpiresAt = expiresAt;
        Status = ESubscriptionStatus.Active;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        if (Status == ESubscriptionStatus.Canceled) return;
        if (Status == ESubscriptionStatus.Expired) return;

        Status = ESubscriptionStatus.Canceled;
        MarkAsUpdated();
    }

    public void Expire()
    {
        if (Status == ESubscriptionStatus.Expired) return;
        if (Status == ESubscriptionStatus.Canceled) return;

        Status = ESubscriptionStatus.Expired;
        MarkAsUpdated();
    }


}
