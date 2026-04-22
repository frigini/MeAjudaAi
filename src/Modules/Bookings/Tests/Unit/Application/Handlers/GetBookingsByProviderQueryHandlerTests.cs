using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class GetBookingsByProviderQueryHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IProviderScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<ILogger<GetBookingsByProviderQueryHandler>> _loggerMock = new();
    private readonly GetBookingsByProviderQueryHandler _sut;

    public GetBookingsByProviderQueryHandlerTests()
    {
        _sut = new GetBookingsByProviderQueryHandler(
            _bookingRepoMock.Object,
            _scheduleRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_BookingsForProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 25);

        var bookings = new List<Booking>
        {
            Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
                TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))),
            Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
                TimeSlot.Create(new TimeOnly(14, 0), new TimeOnly(15, 0)))
        };
        bookings.ForEach(b => b.ClearDomainEvents());

        _bookingRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings.AsReadOnly());

        var schedule = ProviderSchedule.Create(providerId);
        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByProviderQuery(providerId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(b => b.ProviderId.Should().Be(providerId));
    }

    [Fact]
    public async Task HandleAsync_Should_Return_EmptyList_When_NoBookings()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _bookingRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>().AsReadOnly());

        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null);

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByProviderQuery(providerId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
