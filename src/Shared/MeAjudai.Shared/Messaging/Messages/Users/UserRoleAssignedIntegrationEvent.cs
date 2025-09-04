using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Published when a role is assigned to a user
/// </summary>
public sealed record UserRoleAssignedIntegrationEvent(
    string Source,
    Guid UserId,
    string Email,
    string RoleName,
    string? TierName,
    IEnumerable<string> AllCurrentRoles
) : IntegrationEvent(Source);