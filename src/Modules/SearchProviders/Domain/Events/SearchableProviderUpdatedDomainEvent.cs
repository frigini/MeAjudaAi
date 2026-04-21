using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.SearchProviders.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando as informações de um provedor pesquisável são atualizadas.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record SearchableProviderUpdatedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId
) : DomainEvent(AggregateId, Version);
