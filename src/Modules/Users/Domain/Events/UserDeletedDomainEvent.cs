using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Evento de domínio emitido quando um usuário é deletado (soft delete)
/// </summary>
public record UserDeletedDomainEvent(
    Guid AggregateId,
    int Version
) : DomainEvent(AggregateId, Version);
