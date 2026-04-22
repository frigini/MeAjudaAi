using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um agendamento é cancelado.
/// </summary>
[ExcludeFromCodeCoverage]
public record BookingCancelledDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    Guid ClientId,
    string Reason
) : DomainEvent(AggregateId, Version);
