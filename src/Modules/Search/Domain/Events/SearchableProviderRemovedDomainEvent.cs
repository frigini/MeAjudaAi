using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Search.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um provedor pesquisável é removido do índice.
/// </summary>
public sealed record SearchableProviderRemovedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId
) : DomainEvent(AggregateId, Version);
