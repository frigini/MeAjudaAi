using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Search.Domain.Events;

/// <summary>
/// Domain event raised when a searchable provider is removed from the index.
/// </summary>
public sealed record SearchableProviderRemovedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId
) : DomainEvent(AggregateId, Version);
