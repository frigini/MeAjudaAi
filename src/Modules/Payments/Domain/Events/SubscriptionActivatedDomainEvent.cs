using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

public record SubscriptionActivatedDomainEvent(
    Guid SubscriptionId, 
    Guid ProviderId, 
    string ExternalSubscriptionId
) : DomainEvent(SubscriptionId, DomainConstants.InitialVersion);
