using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

/// <summary>
/// Evento disparado quando uma assinatura expira.
/// </summary>
public record SubscriptionExpiredDomainEvent(
    Guid SubscriptionId, 
    Guid ProviderId
) : DomainEvent(SubscriptionId, DomainConstants.InitialVersion);
