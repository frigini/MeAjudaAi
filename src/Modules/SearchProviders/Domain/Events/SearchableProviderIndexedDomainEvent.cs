using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.SearchProviders.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando uma entrada de provedor pesquisável é criada.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record SearchableProviderIndexedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    string Name,
    double Latitude,
    double Longitude
) : DomainEvent(AggregateId, Version);
