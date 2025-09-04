using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Published when a user account is deactivated
/// </summary>
public record UserDeactivatedDomainEvent
(
    Guid AggregateId,
    int Version,
    string Reason,
    DateTime DeactivatedAt = default
) : DomainEvent(AggregateId, Version);