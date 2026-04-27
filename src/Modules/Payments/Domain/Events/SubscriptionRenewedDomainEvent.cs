using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

public record SubscriptionRenewedDomainEvent(Guid SubscriptionId, Guid ProviderId, DateTime NewExpiresAt, int Version) 
    : DomainEvent(SubscriptionId, Version);
