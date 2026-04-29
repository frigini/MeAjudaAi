using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Streaming;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Events;

/// <summary>
/// Handler paralelo que publica eventos de mudança de status de reserva via SSE.
/// </summary>
public class BookingRealtimeEventsHandler(
    ISseHub<BookingStatusSseDto> sseHub,
    ILogger<BookingRealtimeEventsHandler> logger) :
    IEventHandler<BookingCreatedDomainEvent>,
    IEventHandler<BookingConfirmedDomainEvent>,
    IEventHandler<BookingCancelledDomainEvent>,
    IEventHandler<BookingRejectedDomainEvent>,
    IEventHandler<BookingCompletedDomainEvent>
{
    public async Task HandleAsync(BookingCreatedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, "Created", "Reserva criada com sucesso");
    }

    public async Task HandleAsync(BookingConfirmedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, "Confirmed", "Reserva confirmada pelo prestador");
    }

    public async Task HandleAsync(BookingCancelledDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, "Cancelled", "Reserva cancelada");
    }

    public async Task HandleAsync(BookingRejectedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, "Rejected", "Reserva rejeitada pelo prestador");
    }

    public async Task HandleAsync(BookingCompletedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, "Completed", "Serviço finalizado");
    }

    private async Task PublishUpdate(Guid bookingId, string status, string message)
    {
        var topic = SseTopic.ForBooking(bookingId);
        var data = new BookingStatusSseDto(bookingId, status, DateTime.UtcNow, message);
        
        logger.LogInformation("Streaming update: Booking {BookingId} transitioned to {Status}", bookingId, status);
        await sseHub.PublishAsync(topic, data);
    }
}
