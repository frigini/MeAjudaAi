using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Published when a role is revoked from a user
/// </summary>
public sealed record UserRoleRevokedIntegrationEvent(
    string Source,
    Guid UserId,
    string Email,
    string RevokedRoleName,
    IEnumerable<string> RemainingRoles
) : IntegrationEvent(Source);