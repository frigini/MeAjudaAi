using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class GetBookingsByClientQueryHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IProviderScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<ILogger<GetBookingsByClientQueryHandler>> _loggerMock = new();
    private readonly GetBookingsByClientQueryHandler _sut;

    public GetBookingsByClientQueryHandlerTests()
    {
        _sut = new GetBookingsByClientQueryHandler(
            _bookingRepoMock.Object,
            _scheduleRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_BookingsForClient()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 25);

        var bookings = new List<Booking>
        {
            Booking.Create(providerId, clientId, Guid.NewGuid(), date,
                TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))),
            Booking.Create(providerId, clientId, Guid.NewGuid(), date,
                TimeSlot.Create(new TimeOnly(14, 0), new TimeOnly(15, 0)))
        };
        bookings.ForEach(b => b.ClearDomainEvents());

        _bookingRepoMock.Setup(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings.AsReadOnly());

        var schedule = ProviderSchedule.Create(providerId);
        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByClientQuery(clientId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(b => b.ClientId.Should().Be(clientId));
    }

    [Fact]
    public async Task HandleAsync_Should_Return_EmptyList_When_NoBookings()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _bookingRepoMock.Setup(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>().AsReadOnly());

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByClientQuery(clientId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
