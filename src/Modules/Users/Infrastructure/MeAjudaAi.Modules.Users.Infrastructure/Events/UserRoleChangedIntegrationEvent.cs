using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Infrastructure.Events;

public record UserRoleChangedIntegrationEvent(
    Guid UserId,
    string PreviousRole,
    string NewRole,
    string ChangedBy,
    DateTime ChangedAt
) : IntegrationEvent("Users");