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

        ExternalSubscriptionId = externalSubscriptionId;
        StartedAt = startedAt;
        ExpiresAt = expiresAt;
        Status = ESubscriptionStatus.Active;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        if (Status == ESubscriptionStatus.Canceled) return;

        Status = ESubscriptionStatus.Canceled;
        MarkAsUpdated();
    }

    public void Expire()
    {
        if (Status == ESubscriptionStatus.Expired) return;

        Status = ESubscriptionStatus.Expired;
        MarkAsUpdated();
    }


}
