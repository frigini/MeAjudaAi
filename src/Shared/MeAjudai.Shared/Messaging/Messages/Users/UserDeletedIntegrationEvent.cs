using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Published when a user is deleted (soft delete)
/// </summary>
public sealed record UserDeletedIntegrationEvent
(
    string Source,
    Guid UserId,
    DateTime DeletedAt
) : IntegrationEvent(Source);