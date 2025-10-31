using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Publicado quando um novo usuário se registra no sistema
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
