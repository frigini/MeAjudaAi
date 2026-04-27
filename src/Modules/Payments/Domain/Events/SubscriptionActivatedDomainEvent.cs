using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

public record SubscriptionActivatedDomainEvent(Guid SubscriptionId, Guid ProviderId, string ExternalSubscriptionId, int Version) 
    : DomainEvent(SubscriptionId, Version);
