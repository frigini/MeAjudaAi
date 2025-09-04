using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Published when a user account is deactivated
/// </summary>
public sealed record UserDeactivatedIntegrationEvent(
    string Source,
    Guid UserId,
    string Email,
    string Reason,
    DateTime DeactivatedAt
) : IntegrationEvent(Source);