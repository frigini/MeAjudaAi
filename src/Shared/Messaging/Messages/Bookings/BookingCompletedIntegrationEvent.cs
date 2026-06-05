using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Bookings;

/// <summary>
/// Evento de integração disparado quando um agendamento é completado.
/// </summary>
[ExcludeFromCodeCoverage]
public record BookingCompletedIntegrationEvent(
    string Source,
    Guid BookingId,
    Guid ProviderId,
    Guid ClientId
) : IntegrationEvent(Source);
