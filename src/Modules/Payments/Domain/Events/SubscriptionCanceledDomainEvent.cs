using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

public record SubscriptionCanceledDomainEvent(
    Guid SubscriptionId, 
    Guid ProviderId
) : DomainEvent(SubscriptionId, 0);
