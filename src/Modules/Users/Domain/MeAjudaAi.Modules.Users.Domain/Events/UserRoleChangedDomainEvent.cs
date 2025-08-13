using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

public record UserRoleChangedDomainEvent(
    Guid AggregateId,
    int Version,
    string PreviousRoles,
    string NewRole,
    string ChangedBy
) : DomainEvent(AggregateId, Version);