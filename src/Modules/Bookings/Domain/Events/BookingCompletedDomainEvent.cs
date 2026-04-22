using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um agendamento é marcado como concluído.
/// </summary>
public record BookingCompletedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    Guid ClientId
) : DomainEvent(AggregateId, Version);
