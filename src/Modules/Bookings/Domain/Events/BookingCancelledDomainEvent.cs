using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Bookings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um agendamento é cancelado.
/// </summary>
public record BookingCancelledDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    Guid ClientId,
    string Reason
) : DomainEvent(AggregateId, Version);
