using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Publicado quando um usuário atualiza suas informações de perfil
/// </summary>
[ExcludeFromCodeCoverage]
[HighVolumeEvent(20)]
public sealed record UserProfileUpdatedIntegrationEvent(
    string Source,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime UpdatedAt
) : IntegrationEvent(Source);
