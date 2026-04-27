using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um novo agendamento é criado.
/// </summary>
public record BookingCreatedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    Guid ClientId,
    Guid ServiceId,
    DateOnly Date
) : DomainEvent(AggregateId, Version);
