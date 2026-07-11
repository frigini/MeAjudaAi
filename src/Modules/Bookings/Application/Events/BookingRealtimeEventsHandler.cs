using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Application.Constants;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Streaming;
using MeAjudaAi.Shared.Streaming.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Events;

/// <summary>
/// Manipulador de eventos de domínio responsável por publicar atualizações de estado dos agendamentos 
/// em tempo real via Server-Sent Events (SSE).
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
        await PublishUpdate(@event.AggregateId, EBookingStatus.Pending, BookingSseMessages.Created, cancellationToken);
    }

    public async Task HandleAsync(BookingConfirmedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, EBookingStatus.Confirmed, BookingSseMessages.Confirmed, cancellationToken);
    }

    public async Task HandleAsync(BookingCancelledDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, EBookingStatus.Cancelled, BookingSseMessages.Cancelled, cancellationToken);
    }

    public async Task HandleAsync(BookingRejectedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, EBookingStatus.Rejected, BookingSseMessages.Rejected, cancellationToken);
    }

    public async Task HandleAsync(BookingCompletedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await PublishUpdate(@event.AggregateId, EBookingStatus.Completed, BookingSseMessages.Completed, cancellationToken);
    }

    private async Task PublishUpdate(Guid bookingId, EBookingStatus status, string message, CancellationToken cancellationToken)
    {
        var topic = SseTopic.ForBooking(bookingId);
        var data = new BookingStatusSseDto(bookingId, status.ToString(), DateTime.UtcNow, message);
        
        logger.LogInformation("Streaming update: Booking {BookingId} transitioned to {Status}", bookingId, status);
        await sseHub.PublishAsync(topic, data, cancellationToken);
    }
}

