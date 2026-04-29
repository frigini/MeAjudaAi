using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Events;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Streaming;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Events;

public class BookingRealtimeEventsHandlerTests
{
    private readonly Mock<ISseHub<BookingStatusSseDto>> _sseHubMock = new();
    private readonly Mock<ILogger<BookingRealtimeEventsHandler>> _loggerMock = new();
    private readonly BookingRealtimeEventsHandler _sut;

    public BookingRealtimeEventsHandlerTests()
    {
        _sseHubMock
            .Setup(h => h.PublishAsync(It.IsAny<string>(), It.IsAny<BookingStatusSseDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _sut = new BookingRealtimeEventsHandler(_sseHubMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_BookingCreated_ShouldPublishCreatedStatus()
    {
        var bookingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var @event = new BookingCreatedDomainEvent(bookingId, 1, providerId, clientId, serviceId, DateOnly.FromDateTime(DateTime.Today));

        await _sut.HandleAsync(@event);

        _sseHubMock.Verify(h => h.PublishAsync(
            SseTopic.ForBooking(bookingId),
            It.Is<BookingStatusSseDto>(d => d.BookingId == bookingId && d.Status == "Created"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BookingConfirmed_ShouldPublishConfirmedStatus()
    {
        var bookingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var @event = new BookingConfirmedDomainEvent(bookingId, 1, providerId, clientId);

        await _sut.HandleAsync(@event);

        _sseHubMock.Verify(h => h.PublishAsync(
            SseTopic.ForBooking(bookingId),
            It.Is<BookingStatusSseDto>(d => d.Status == "Confirmed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BookingCancelled_ShouldPublishCancelledStatus()
    {
        var bookingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var @event = new BookingCancelledDomainEvent(bookingId, 1, providerId, clientId, "Test cancellation");

        await _sut.HandleAsync(@event);

        _sseHubMock.Verify(h => h.PublishAsync(
            SseTopic.ForBooking(bookingId),
            It.Is<BookingStatusSseDto>(d => d.Status == "Cancelled"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BookingRejected_ShouldPublishRejectedStatus()
    {
        var bookingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var @event = new BookingRejectedDomainEvent(bookingId, 1, providerId, clientId, "Test rejection");

        await _sut.HandleAsync(@event);

        _sseHubMock.Verify(h => h.PublishAsync(
            SseTopic.ForBooking(bookingId),
            It.Is<BookingStatusSseDto>(d => d.Status == "Rejected"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BookingCompleted_ShouldPublishCompletedStatus()
    {
        var bookingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var @event = new BookingCompletedDomainEvent(bookingId, 1, providerId, clientId);

        await _sut.HandleAsync(@event);

        _sseHubMock.Verify(h => h.PublishAsync(
            SseTopic.ForBooking(bookingId),
            It.Is<BookingStatusSseDto>(d => d.Status == "Completed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}