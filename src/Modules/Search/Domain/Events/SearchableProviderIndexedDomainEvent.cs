using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Search.Domain.Events;

/// <summary>
/// Domain event raised when a searchable provider entry is created.
/// </summary>
public sealed record SearchableProviderIndexedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    string Name,
    double Latitude,
    double Longitude
) : DomainEvent(AggregateId, Version);
