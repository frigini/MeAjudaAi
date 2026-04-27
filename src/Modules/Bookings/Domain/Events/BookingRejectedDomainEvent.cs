using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Bookings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um agendamento é rejeitado pelo prestador.
/// </summary>
public record BookingRejectedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    Guid ClientId,
    string Reason
) : DomainEvent(AggregateId, Version);
