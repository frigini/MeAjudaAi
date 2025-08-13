using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Infrastructure.Events;

public record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime RegisteredAt
) : IntegrationEvent("Users");