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
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var schedule = ProviderSchedule.Create(providerId, "UTC");
        
        var slotStart = new TimeOnly(8, 0);
        var slotEnd = new TimeOnly(10, 0);
        
        schedule.SetAvailability(Availability.Create(date.DayOfWeek, 
            [TimeSlot.Create(slotStart, slotEnd)]));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepoMock.Setup(x => x.GetActiveByProviderAndDateAsync(providerId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().HaveCount(1);
        
        var returnedSlot = result.Value.Slots.First();
        returnedSlot.Start.Offset.Should().Be(TimeSpan.Zero);
        returnedSlot.End.Offset.Should().Be(TimeSpan.Zero);
        returnedSlot.Start.DateTime.Should().Be(date.ToDateTime(slotStart));
        returnedSlot.End.DateTime.Should().Be(date.ToDateTime(slotEnd));
    }

    [Fact]
    public async Task HandleAsync_Should_FilterOut_BookedSlots()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var schedule = ProviderSchedule.Create(providerId, "UTC");
        schedule.SetAvailability(Availability.Create(date.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0))]));

        var existingBooking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(8, 30), new TimeOnly(9, 30)));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepoMock.Setup(x => x.GetActiveByProviderAndDateAsync(providerId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { existingBooking });

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().HaveCount(2);
        var slots = result.Value.Slots.ToList();
        
        slots[0].Start.Offset.Should().Be(TimeSpan.Zero);
        slots[0].End.Offset.Should().Be(TimeSpan.Zero);
        slots[0].Start.DateTime.Should().Be(date.ToDateTime(new TimeOnly(8, 0)));
        slots[0].End.DateTime.Should().Be(date.ToDateTime(new TimeOnly(8, 30)));
        
        slots[1].Start.DateTime.Should().Be(date.ToDateTime(new TimeOnly(9, 30)));
        slots[1].End.DateTime.Should().Be(date.ToDateTime(new TimeOnly(10, 0)));
    }

    [Fact]
    public async Task HandleAsync_Should_Handle_NullSchedule()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null);

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnNoSlots_When_BookingCoversEntireSlot()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var schedule = ProviderSchedule.Create(providerId);
        var slotStart = new TimeOnly(8, 0);
        var slotEnd = new TimeOnly(10, 0);
        schedule.SetAvailability(Availability.Create(date.DayOfWeek, 
            [TimeSlot.Create(slotStart, slotEnd)]));

        var existingBooking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(slotStart, slotEnd));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepoMock.Setup(x => x.GetActiveByProviderAndDateAsync(providerId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { existingBooking });

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_Should_Ignore_BookingsOnDifferentDate()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var schedule = ProviderSchedule.Create(providerId);
        var slotStart = new TimeOnly(8, 0);
        var slotEnd = new TimeOnly(10, 0);
        schedule.SetAvailability(Availability.Create(date.DayOfWeek, 
            [TimeSlot.Create(slotStart, slotEnd)]));

        // Simulamos que o repositório retorna uma lista vazia, o que é o esperado para uma data diferente.
        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepoMock.Setup(x => x.GetActiveByProviderAndDateAsync(providerId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().HaveCount(1);
    }
}

