using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um agendamento é rejeitado pelo prestador.
/// </summary>
[ExcludeFromCodeCoverage]
public record BookingRejectedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ProviderId,
    Guid ClientId,
    string Reason
) : DomainEvent(AggregateId, Version);
