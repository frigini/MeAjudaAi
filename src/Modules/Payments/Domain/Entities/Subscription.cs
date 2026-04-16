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
    public string? ExternalCustomerId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public ESubscriptionStatus Status { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public void Activate(string externalSubscriptionId, string externalCustomerId, DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(externalSubscriptionId))
            throw new ArgumentException("ExternalSubscriptionId cannot be empty.", nameof(externalSubscriptionId));

        if (string.IsNullOrWhiteSpace(externalCustomerId))
            throw new ArgumentException("ExternalCustomerId cannot be empty.", nameof(externalCustomerId));

        if (expiresAt.HasValue && (expiresAt.Value == default || expiresAt.Value <= DateTime.UtcNow))
            throw new ArgumentException("ExpiresAt must be a valid future date.", nameof(expiresAt));

        // Se já está ativa e os IDs são os mesmos, nada a fazer (idempotência)
        if (Status == ESubscriptionStatus.Active && 
            ExternalSubscriptionId == externalSubscriptionId && 
            ExternalCustomerId == externalCustomerId) 
            return;

        // Se estiver terminalmente fechada, não permite reativar por este método (deveria criar nova)
        if (Status == ESubscriptionStatus.Canceled)
            throw new InvalidOperationException("Cannot activate a canceled subscription.");
        
        if (Status == ESubscriptionStatus.Expired)
            throw new InvalidOperationException("Cannot activate an expired subscription.");

        ExternalSubscriptionId = externalSubscriptionId;
        ExternalCustomerId = externalCustomerId;
        Status = ESubscriptionStatus.Active;
        ExpiresAt = expiresAt;
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
    public void Renew(DateTime newExpiresAt)
    {
        if (Status != ESubscriptionStatus.Active)
            throw new InvalidOperationException($"Cannot renew a subscription with status {Status}. Only Active subscriptions can be renewed.");

        if (newExpiresAt <= DateTime.UtcNow)
            throw new ArgumentException("New expiration date must be a valid future date.", nameof(newExpiresAt));

        if (ExpiresAt.HasValue && newExpiresAt <= ExpiresAt.Value)
            throw new ArgumentException("New expiration date must be after current expiration date.", nameof(newExpiresAt));

        ExpiresAt = newExpiresAt;
        Status = ESubscriptionStatus.Active;
        MarkAsUpdated();
    }
}
