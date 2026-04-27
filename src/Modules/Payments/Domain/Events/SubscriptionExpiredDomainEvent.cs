using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

public record SubscriptionExpiredDomainEvent(Guid SubscriptionId, Guid ProviderId, int Version) 
    : DomainEvent(SubscriptionId, Version);
