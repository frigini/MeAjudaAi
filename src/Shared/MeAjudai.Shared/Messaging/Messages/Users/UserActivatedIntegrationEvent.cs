using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Published when a user account is activated
/// </summary>
public sealed record UserActivatedIntegrationEvent(
    string Source,
    Guid UserId,
    string Email,
    string ActivatedBy,
    DateTime ActivatedAt
) : IntegrationEvent(Source);