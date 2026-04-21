using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class GetProviderAvailabilityQueryHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IProviderScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<ILogger<GetProviderAvailabilityQueryHandler>> _loggerMock = new();
    private readonly GetProviderAvailabilityQueryHandler _sut;

    public GetProviderAvailabilityQueryHandlerTests()
    {
        _sut = new GetProviderAvailabilityQueryHandler(
            _bookingRepoMock.Object,
            _scheduleRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnAvailableSlots_When_NoBookingsExist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 22);
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var schedule = ProviderSchedule.Create(providerId);
        
        // Slot das 08:00 às 10:00
        var slotStart = new TimeOnly(8, 0);
        var slotEnd = new TimeOnly(10, 0);
        
        schedule.SetAvailability(Availability.Create(date.DayOfWeek, 
            [TimeSlot.Create(slotStart, slotEnd)]));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().HaveCount(1);
        
        var returnedSlot = result.Value.Slots.First();
        returnedSlot.Start.Should().Be(date.ToDateTime(slotStart));
        returnedSlot.End.Should().Be(date.ToDateTime(slotEnd));
    }

    [Fact]
    public async Task HandleAsync_Should_FilterOut_BookedSlots()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 22);
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var schedule = ProviderSchedule.Create(providerId);
        // Slot das 08:00 às 10:00
        schedule.SetAvailability(Availability.Create(date.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0))]));

        // Já existe um booking das 08:30 às 09:30 nesta data
        var existingBooking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(8, 30), new TimeOnly(9, 30)));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { existingBooking });

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().BeEmpty();
    }
}
