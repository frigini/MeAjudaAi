using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Infrastructure.Events;

[CriticalEvent]
public record UserLockedOutIntegrationEvent(
    Guid UserId,
    string Email,
    string Reason,
    DateTime LockedAt,
    DateTime? UnlockAt
) : IntegrationEvent("Users");