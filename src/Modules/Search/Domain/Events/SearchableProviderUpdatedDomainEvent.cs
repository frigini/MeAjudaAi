using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Search.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando as informações de um provedor pesquisável são atualizadas.
/// </summary>
public sealed record SearchableProviderUpdatedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId
) : DomainEvent(AggregateId, Version);
