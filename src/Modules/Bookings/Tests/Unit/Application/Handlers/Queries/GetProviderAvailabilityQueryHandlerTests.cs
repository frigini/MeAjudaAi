using MeAjudaAi.Modules.Bookings.Application.Handlers.Queries;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers.Queries;

public class GetProviderAvailabilityQueryHandlerTests
{
    private readonly Mock<IBookingQueries> _bookingQueriesMock = new();
    private readonly Mock<IProviderScheduleQueries> _scheduleQueriesMock = new();
    private readonly Mock<ILogger<GetProviderAvailabilityQueryHandler>> _loggerMock = new();
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock = new();
    private readonly GetProviderAvailabilityQueryHandler _sut;

    public GetProviderAvailabilityQueryHandlerTests()
    {
        _sut = new GetProviderAvailabilityQueryHandler(
            _bookingQueriesMock.Object,
            _scheduleQueriesMock.Object,
            _loggerMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnAvailableSlots_When_NoBookingsExist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var slotStart = new TimeOnly(8, 0);
        var slotEnd = new TimeOnly(10, 0);
        
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(providerId)
            .WithTimeZoneId("UTC")
            .WithSingleSlot(date.DayOfWeek, slotStart, slotEnd)
            .Build();

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingQueriesMock.Setup(x => x.GetActiveByProviderAndDateAsync(providerId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Slots.Should().HaveCount(1);
        
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
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(providerId)
            .WithTimeZoneId("UTC")
            .WithSingleSlot(date.DayOfWeek, new TimeOnly(8, 0), new TimeOnly(10, 0))
            .Build();

        var existingBooking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(date)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 30)).WithEnd(new TimeOnly(9, 30)).Build())
            .Build();

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingQueriesMock.Setup(x => x.GetActiveByProviderAndDateAsync(providerId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { existingBooking });

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Slots.Should().HaveCount(2);
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
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
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
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var slotStart = new TimeOnly(8, 0);
        var slotEnd = new TimeOnly(10, 0);
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(providerId)
            .WithTimeZoneId("UTC")
            .WithSingleSlot(date.DayOfWeek, slotStart, slotEnd)
            .Build();

        var existingBooking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(date)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(slotStart).WithEnd(slotEnd).Build())
            .Build();

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingQueriesMock.Setup(x => x.GetActiveByProviderAndDateAsync(providerId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { existingBooking });

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Slots.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_Should_Ignore_BookingsOnDifferentDate()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var slotStart = new TimeOnly(8, 0);
        var slotEnd = new TimeOnly(10, 0);
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(providerId)
            .WithTimeZoneId("UTC")
            .WithSingleSlot(date.DayOfWeek, slotStart, slotEnd)
            .Build();

        var otherDate = date.AddDays(2);
        var bookingOnOtherDate = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(otherDate)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 0)).WithEnd(new TimeOnly(9, 0)).Build())
            .Build();

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingQueriesMock.Setup(x => x.GetActiveByProviderAndDateAsync(providerId, It.Is<DateOnly>(d => d == date), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());
        _bookingQueriesMock.Setup(x => x.GetActiveByProviderAndDateAsync(providerId, It.Is<DateOnly>(d => d == otherDate), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { bookingOnOtherDate });

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Slots.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_WhenNoDayScheduleFound_ShouldReturnEmptySlots()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var query = new GetProviderAvailabilityQuery(providerId, date, Guid.NewGuid());

        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(providerId)
            .WithTimeZoneId("UTC")
            .Build();

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Slots.Should().BeEmpty();
    }
}