using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Ratings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando uma avaliação é aprovada.
/// </summary>
[ExcludeFromCodeCoverage]
public record ReviewApprovedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    int Rating,
    string? Comment
) : DomainEvent(AggregateId, Version);
