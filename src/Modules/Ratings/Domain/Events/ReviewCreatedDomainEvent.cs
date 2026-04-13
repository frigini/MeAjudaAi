using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Ratings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando uma nova avaliação é criada.
/// </summary>
public record ReviewCreatedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    Guid CustomerId,
    int Rating,
    string? Comment
) : DomainEvent(AggregateId, Version);
