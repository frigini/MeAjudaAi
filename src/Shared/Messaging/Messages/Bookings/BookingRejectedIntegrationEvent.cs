using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Bookings;

/// <summary>
/// Evento de integração disparado quando um agendamento é rejeitado.
/// </summary>
[ExcludeFromCodeCoverage]
[CriticalEvent]
public record BookingRejectedIntegrationEvent(
    string Source,
    Guid BookingId,
    Guid ProviderId,
    Guid ClientId,
    string Reason
) : IntegrationEvent(Source);
