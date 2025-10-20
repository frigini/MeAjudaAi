using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Publicado quando um usuário é excluído (soft delete)
/// </summary>
public sealed record UserDeletedIntegrationEvent
(
    string Source,
    Guid UserId,
    DateTime DeletedAt
) : IntegrationEvent(Source);
