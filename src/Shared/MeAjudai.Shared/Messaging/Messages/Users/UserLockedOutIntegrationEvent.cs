using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Published when a user account is locked out due to security reasons
/// </summary>
public sealed record UserLockedOutIntegrationEvent(
    string Source,
    Guid UserId,
    string Email,
    string Reason,
    DateTime LockedOutAt,
    DateTime? UnlockAt
) : IntegrationEvent(Source);