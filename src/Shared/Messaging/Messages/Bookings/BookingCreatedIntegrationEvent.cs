using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Bookings;

/// <summary>
/// Evento de integração disparado quando um booking é criado.
/// </summary>
[ExcludeFromCodeCoverage]
[CriticalEvent]
public record BookingCreatedIntegrationEvent(
    string Source,
    Guid BookingId,
    Guid ProviderId,
    Guid ClientId,
    Guid ServiceId,
    DateOnly Date
) : IntegrationEvent(Source);
