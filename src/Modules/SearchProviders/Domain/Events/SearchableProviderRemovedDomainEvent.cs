using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.SearchProviders.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um provedor pesquisável é removido do índice.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record SearchableProviderRemovedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId
) : DomainEvent(AggregateId, Version);
