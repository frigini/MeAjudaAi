using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Ratings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando uma avaliação é rejeitada.
/// </summary>
[ExcludeFromCodeCoverage]
public record ReviewRejectedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    string Reason
) : DomainEvent(AggregateId, Version);
