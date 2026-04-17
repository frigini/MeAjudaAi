using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

/// <summary>
/// Evento disparado quando uma assinatura é renovada.
/// </summary>
public record SubscriptionRenewedDomainEvent(
    Guid SubscriptionId, 
    Guid ProviderId, 
    DateTime NewExpiresAt
) : DomainEvent(SubscriptionId, DomainConstants.InitialVersion);
