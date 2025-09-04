using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Published when a new user registers in the system
/// </summary>
public sealed record UserRegisteredIntegrationEvent(
    string Source,
    Guid UserId,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    string KeycloakId,
    IEnumerable<string> Roles,
    DateTime RegisteredAt
) : IntegrationEvent(Source);