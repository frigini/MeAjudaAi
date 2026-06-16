using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Publicado quando um usuário é excluído (soft delete)
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record UserDeletedIntegrationEvent
(
    string Source,
    Guid UserId,
    string Email,
    string FirstName,
    DateTime DeletedAt
) : IntegrationEvent(Source);
