using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

[Trait("Category", "Unit")]
public class CompleteBookingCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<ILogger<CompleteBookingCommandHandler>> _loggerMock = new();
    private readonly CompleteBookingCommandHandler _sut;

    public CompleteBookingCommandHandlerTests()
    {
        _sut = new CompleteBookingCommandHandler(
            _bookingRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Complete_When_BookingIsConfirmed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.Confirm();
        booking.ClearDomainEvents();

        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, false, providerId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Completed);
        _bookingRepoMock.Verify(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnBadRequest_When_BookingStateIsInvalidForCompletion()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));

        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, true, null, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        result.Error.Code.Should().Be(ErrorCodes.Bookings.InvalidState);
        _bookingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_UserIsNotOwner()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.Confirm();
        booking.ClearDomainEvents();

        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, false, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _bookingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_BookingNotFound()
    {
        // Arrange
        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(Guid.NewGuid(), false, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task HandleAsync_Should_Complete_When_AdminAndBookingIsConfirmed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.Confirm();
        booking.ClearDomainEvents();

        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, true, null, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Completed);
        _bookingRepoMock.Verify(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_409_When_UpdateAsync_Throws_ConcurrencyConflictException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.Confirm();
        booking.ClearDomainEvents();
        
        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _bookingRepoMock.Setup(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException());

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, false, providerId, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        _bookingRepoMock.Verify(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
    }
}
