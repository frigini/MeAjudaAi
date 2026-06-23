using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Application.ModuleApi;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Application")]
public class BookingsModuleApiTests
{
    private readonly Mock<IBookingQueries> _bookingQueriesMock;
    private readonly BookingsModuleApi _api;

    public BookingsModuleApiTests()
    {
        _bookingQueriesMock = new Mock<IBookingQueries>();
        _api = new BookingsModuleApi(_bookingQueriesMock.Object);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCanConnect_ReturnsTrue()
    {
        // Arrange
        _bookingQueriesMock.Setup(q => q.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _api.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCannotConnect_ReturnsFalse()
    {
        // Arrange
        _bookingQueriesMock.Setup(q => q.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _api.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenQueryReturnsTrue_ShouldReturnSuccessTrue()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.HasCompletedBookingAsync(clientId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _api.HasCompletedBookingAsync(clientId, providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenQueryReturnsFalse_ShouldReturnSuccessFalse()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.HasCompletedBookingAsync(clientId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _api.HasCompletedBookingAsync(clientId, providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenNotFound_ShouldReturnSuccessWithNull()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await _api.GetBookingByIdAsync(bookingId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenFound_ShouldReturnMappedDto()
    {
        // Arrange
        var booking = new BookingBuilder()
            .WithProviderId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .WithStatus(EBookingStatus.Confirmed)
            .Build();
        _bookingQueriesMock.Setup(q => q.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _api.GetBookingByIdAsync(booking.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task GetProviderBookingsAsync_WhenQueryReturnsItems_ShouldReturnMappedDtos()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(1);

        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(start.Date))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .WithStatus(EBookingStatus.Confirmed)
            .Build();

        _bookingQueriesMock.Setup(q => q.GetByProviderAndPeriodAsync(providerId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { booking });

        // Act
        var result = await _api.GetProviderBookingsAsync(providerId, start, end);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task GetProviderBookingsAsync_WhenQueryReturnsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(1);
            
        _bookingQueriesMock.Setup(q => q.GetByProviderAndPeriodAsync(providerId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _api.GetProviderBookingsAsync(providerId, start, end);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
