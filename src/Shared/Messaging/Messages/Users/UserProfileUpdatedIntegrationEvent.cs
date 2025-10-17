using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Publicado quando um usuário atualiza suas informações de perfil
/// </summary>
public sealed record UserProfileUpdatedIntegrationEvent(
    string Source,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime UpdatedAt
) : IntegrationEvent(Source);