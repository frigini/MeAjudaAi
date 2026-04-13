using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Ratings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando uma avaliação é aprovada.
/// </summary>
public record ReviewApprovedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    int Rating,
    string? Comment
) : DomainEvent(AggregateId, Version);
