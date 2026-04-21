using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Ratings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando uma nova avaliação é criada.
/// </summary>
[ExcludeFromCodeCoverage]
public record ReviewCreatedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    Guid CustomerId,
    int Rating,
    string? Comment
) : DomainEvent(AggregateId, Version);
