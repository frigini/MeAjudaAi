using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Search.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando uma entrada de provedor pesquisável é criada.
/// </summary>
public sealed record SearchableProviderIndexedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    string Name,
    double Latitude,
    double Longitude
) : DomainEvent(AggregateId, Version);
