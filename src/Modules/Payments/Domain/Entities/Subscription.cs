using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Domain.Events;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;
using System.ComponentModel.DataAnnotations.Schema;

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
        : base(Guid.NewGuid())
    {
        if (providerId == Guid.Empty)
            throw new ArgumentException("ProviderId cannot be empty.", nameof(providerId));

        if (string.IsNullOrWhiteSpace(planId))
            throw new ArgumentException("PlanId cannot be null or empty.", nameof(planId));

        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        ProviderId = providerId;
        PlanId = planId;
        Amount = amount;
        Status = ESubscriptionStatus.Pending;
        Version = 1;
    }

    public Subscription(Guid id, Guid providerId, string planId, Money amount, ESubscriptionStatus status, DateTime createdAt)
        : base(id)
    {
        if (providerId == Guid.Empty)
            throw new ArgumentException("ProviderId cannot be empty.", nameof(providerId));

        if (string.IsNullOrWhiteSpace(planId))
            throw new ArgumentException("PlanId cannot be null or empty.", nameof(planId));

        ArgumentNullException.ThrowIfNull(amount);

        ProviderId = providerId;
        PlanId = planId;
        Amount = amount;
        Status = status;
        CreatedAt = createdAt;
        Version = 1;
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

        // Se já está ativa e os IDs são os mesmos
        if (Status == ESubscriptionStatus.Active && 
            ExternalSubscriptionId == externalSubscriptionId && 
            ExternalCustomerId == externalCustomerId)
        {
            // Se o expiresAt foi fornecido e mudou, atualiza
            if (expiresAt.HasValue && expiresAt != ExpiresAt)
            {
                ExpiresAt = expiresAt;
                MarkAsUpdated();
            }
            return;
        }

        // Se estiver terminalmente fechada, não permite reativar por este método (deveria criar nova)
        if (Status == ESubscriptionStatus.Canceled)
            throw new InvalidOperationException("Cannot activate a canceled subscription.");
        
        if (Status == ESubscriptionStatus.Expired)
            throw new InvalidOperationException("Cannot activate an expired subscription.");

        ExternalSubscriptionId = externalSubscriptionId;
        ExternalCustomerId = externalCustomerId;
        Status = ESubscriptionStatus.Active;
        
        if (expiresAt.HasValue)
        {
            ExpiresAt = expiresAt;
        }
        
        Version++;
        MarkAsUpdated();

        AddDomainEvent(new SubscriptionActivatedDomainEvent(Id, ProviderId, ExternalSubscriptionId, Version));
    }

    public void Cancel()
    {
        if (Status == ESubscriptionStatus.Canceled) return;
        if (Status == ESubscriptionStatus.Expired) return;

        Status = ESubscriptionStatus.Canceled;
        Version++;
        MarkAsUpdated();

        AddDomainEvent(new SubscriptionCanceledDomainEvent(Id, ProviderId, Version));
    }

    public void Expire()
    {
        if (Status == ESubscriptionStatus.Expired) return;
        if (Status == ESubscriptionStatus.Canceled) return;

        Status = ESubscriptionStatus.Expired;
        Version++;
        MarkAsUpdated();

        AddDomainEvent(new SubscriptionExpiredDomainEvent(Id, ProviderId, Version));
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
        Version++;
        MarkAsUpdated();

        AddDomainEvent(new SubscriptionRenewedDomainEvent(Id, ProviderId, ExpiresAt.Value, Version));
    }

    public static string MaskExternalId(string externalId)
    {
        if (string.IsNullOrEmpty(externalId)) return string.Empty;
        if (externalId.Length <= 8) return "****" + externalId[^Math.Min(4, externalId.Length)..];
        return externalId[..4] + "****" + externalId[^4..];
    }
}
