using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Evento de domínio emitido quando um usuário é deletado (soft delete)
/// </summary>
[ExcludeFromCodeCoverage]
public record UserDeletedDomainEvent(
    Guid AggregateId,
    int Version
) : DomainEvent(AggregateId, Version);
