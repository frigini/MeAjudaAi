using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Evento de domínio emitido quando o perfil de um usuário é atualizado
/// </summary>
public record UserProfileUpdatedDomainEvent(
    Guid AggregateId,
    int Version,
    string FirstName,
    string LastName
) : DomainEvent(AggregateId, Version);