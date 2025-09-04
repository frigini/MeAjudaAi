using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Published when a user updates their profile information
/// </summary>
public sealed record UserProfileUpdatedIntegrationEvent(
    string Source,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime UpdatedAt
) : IntegrationEvent(Source);