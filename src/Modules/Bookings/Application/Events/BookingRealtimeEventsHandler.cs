using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Streaming;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Events;

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
        await PublishUpdate(@event.AggregateId, "Created", "Reserva criada com sucesso", cancellationToken);
    }

    public async Task HandleAsync(BookingConfirmedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, "Confirmed", "Reserva confirmada pelo prestador", cancellationToken);
    }

    public async Task HandleAsync(BookingCancelledDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, "Cancelled", "Reserva cancelada", cancellationToken);
    }

    public async Task HandleAsync(BookingRejectedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, "Rejected", "Reserva rejeitada pelo prestador", cancellationToken);
    }

    public async Task HandleAsync(BookingCompletedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, "Completed", "Serviço finalizado", cancellationToken);
    }

    private async Task PublishUpdate(Guid bookingId, string status, string message, CancellationToken cancellationToken)
    {
        var topic = SseTopic.ForBooking(bookingId);
        var data = new BookingStatusSseDto(bookingId, status, DateTime.UtcNow, message);
        
        logger.LogInformation("Streaming update: Booking {BookingId} transitioned to {Status}", bookingId, status);
        await sseHub.PublishAsync(topic, data, cancellationToken);
    }
}
