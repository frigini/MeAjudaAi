using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

public record UserDeactivatedDomainEvent
(
    Guid AggregateId,
    int Version,
    string Reason,
    DateTime DeactivatedAt = default
) : DomainEvent(AggregateId, Version);