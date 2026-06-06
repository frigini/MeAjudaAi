using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Bookings;

/// <summary>
/// Evento de integração disparado quando um agendamento é cancelado.
/// </summary>
[ExcludeFromCodeCoverage]
public record BookingCancelledIntegrationEvent(
    string Source,
    Guid BookingId,
    Guid ProviderId,
    Guid ClientId,
    string Reason
) : IntegrationEvent(Source);
