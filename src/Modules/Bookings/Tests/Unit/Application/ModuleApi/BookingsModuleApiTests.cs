using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Application.ModuleApi;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Application")]
public class BookingsModuleApiTests
{
    private readonly Mock<IBookingQueries> _bookingQueriesMock;
    private readonly Mock<ILogger<BookingsModuleApi>> _loggerMock;
    private readonly BookingsModuleApi _api;

    public BookingsModuleApiTests()
    {
        _bookingQueriesMock = new Mock<IBookingQueries>();
        _loggerMock = new Mock<ILogger<BookingsModuleApi>>();
        _api = new BookingsModuleApi(_bookingQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenQueryReturnsTrue_ShouldReturnSuccessTrue()
    {
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.HasCompletedBookingAsync(clientId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _api.HasCompletedBookingAsync(clientId, providerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenQueryReturnsFalse_ShouldReturnSuccessFalse()
    {
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.HasCompletedBookingAsync(clientId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _api.HasCompletedBookingAsync(clientId, providerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenQueryThrowsException_ShouldReturnFailure()
    {
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.HasCompletedBookingAsync(clientId, providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _api.HasCompletedBookingAsync(clientId, providerId);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Error checking booking history.");
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenNotFound_ShouldReturnSuccessWithNull()
    {
        var bookingId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        var result = await _api.GetBookingByIdAsync(bookingId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenFound_ShouldReturnMappedDto()
    {
        var bookingId = Guid.NewGuid();
        var booking = new Booking(bookingId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 
            MeAjudaAi.Modules.Bookings.Domain.ValueObjects.TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)), 
            MeAjudaAi.Contracts.Modules.Bookings.Enums.EBookingStatus.Confirmed, 1);
        _bookingQueriesMock.Setup(q => q.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var result = await _api.GetBookingByIdAsync(bookingId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(bookingId);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenQueryThrowsException_ShouldReturnFailure()
    {
        var bookingId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _api.GetBookingByIdAsync(bookingId);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Error retrieving booking data.");
    }

    [Fact]
    public async Task GetProviderBookingsAsync_WhenQueryReturnsItems_ShouldReturnMappedDtos()
    {
        var providerId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(1);
        
        var booking = new Booking(
            Guid.NewGuid(), providerId, Guid.NewGuid(), Guid.NewGuid(), 
            DateOnly.FromDateTime(start.Date), 
            MeAjudaAi.Modules.Bookings.Domain.ValueObjects.TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)),
            MeAjudaAi.Contracts.Modules.Bookings.Enums.EBookingStatus.Confirmed, 1);
            
        _bookingQueriesMock.Setup(q => q.GetByProviderAndPeriodAsync(providerId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { booking });

        var result = await _api.GetProviderBookingsAsync(providerId, start, end);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task GetProviderBookingsAsync_WhenQueryReturnsEmpty_ShouldReturnEmptyList()
    {
        var providerId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(1);
            
        _bookingQueriesMock.Setup(q => q.GetByProviderAndPeriodAsync(providerId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        var result = await _api.GetProviderBookingsAsync(providerId, start, end);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProviderBookingsAsync_WhenQueryThrowsException_ShouldReturnFailure()
    {
        var providerId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(1);
            
        _bookingQueriesMock.Setup(q => q.GetByProviderAndPeriodAsync(providerId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _api.GetProviderBookingsAsync(providerId, start, end);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Error retrieving bookings.");
    }
}
