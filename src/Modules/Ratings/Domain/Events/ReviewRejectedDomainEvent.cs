using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Ratings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando uma avaliação é rejeitada.
/// </summary>
public record ReviewRejectedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    string Reason
) : DomainEvent(AggregateId, Version);
