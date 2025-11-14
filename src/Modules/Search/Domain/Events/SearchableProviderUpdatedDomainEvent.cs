using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Search.Domain.Events;

/// <summary>
/// Domain event raised when a searchable provider's information is updated.
/// </summary>
public sealed record SearchableProviderUpdatedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId
) : DomainEvent(AggregateId, Version);
