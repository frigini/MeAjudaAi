using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Published when a role is revoked from a user
/// </summary>
public sealed record UserRoleRevokedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid RoleId
) : DomainEvent(AggregateId, Version);