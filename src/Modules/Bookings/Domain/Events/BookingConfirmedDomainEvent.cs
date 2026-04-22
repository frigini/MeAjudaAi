using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um agendamento é confirmado pelo prestador.
/// </summary>
[ExcludeFromCodeCoverage]
public record BookingConfirmedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    Guid ClientId
) : DomainEvent(AggregateId, Version);
