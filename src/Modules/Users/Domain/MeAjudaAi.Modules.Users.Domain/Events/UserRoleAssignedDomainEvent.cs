using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Published when a user's role is assigned or changed
/// </summary>
public record UserRoleAssignedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid RoleId,
    Guid? TierId
) : DomainEvent(AggregateId, Version);