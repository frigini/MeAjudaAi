using MeAjudaAi.Contracts.Bookings.Enums;
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
        await PublishUpdate(@event.AggregateId, EBookingStatus.Pending, "Reserva criada com sucesso", cancellationToken);
    }

    public async Task HandleAsync(BookingConfirmedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, EBookingStatus.Confirmed, "Reserva confirmada pelo prestador", cancellationToken);
    }

    public async Task HandleAsync(BookingCancelledDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, EBookingStatus.Cancelled, "Reserva cancelada", cancellationToken);
    }

    public async Task HandleAsync(BookingRejectedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, EBookingStatus.Rejected, "Reserva rejeitada pelo prestador", cancellationToken);
    }

    public async Task HandleAsync(BookingCompletedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, EBookingStatus.Completed, "Serviço finalizado", cancellationToken);
    }

    private async Task PublishUpdate(Guid bookingId, EBookingStatus status, string message, CancellationToken cancellationToken)
    {
        var topic = SseTopic.ForBooking(bookingId);
        var data = new BookingStatusSseDto(bookingId, status.ToString(), DateTime.UtcNow, message);
        
        logger.LogInformation("Streaming update: Booking {BookingId} transitioned to {Status}", bookingId, status);
        await sseHub.PublishAsync(topic, data, cancellationToken);
    }
}
