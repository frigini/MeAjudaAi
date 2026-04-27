using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Bookings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um agendamento é confirmado pelo prestador.
/// </summary>
public record BookingConfirmedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    Guid ClientId
) : DomainEvent(AggregateId, Version);
