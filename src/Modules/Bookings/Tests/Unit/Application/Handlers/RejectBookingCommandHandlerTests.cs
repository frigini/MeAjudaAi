using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

[Trait("Category", "Unit")]
public class RejectBookingCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<ILogger<RejectBookingCommandHandler>> _loggerMock = new();
    private readonly RejectBookingCommandHandler _sut;

    public RejectBookingCommandHandlerTests()
    {
        _sut = new RejectBookingCommandHandler(
            _bookingRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Reject_When_UserIsProviderOwner()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var command = new RejectBookingCommand(booking.Id, "Reason", false, providerId, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Rejected);
        _bookingRepoMock.Verify(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_UserIsNotOwner()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var command = new RejectBookingCommand(booking.Id, "Reason", false, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_BookingNotFound()
    {
        // Arrange
        var command = new RejectBookingCommand(Guid.NewGuid(), "Reason", false, Guid.NewGuid(), Guid.NewGuid());
        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_BookingAlreadyConfirmed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        booking.Confirm();
        booking.ClearDomainEvents();
        
        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var command = new RejectBookingCommand(booking.Id, "Reason", false, providerId, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Code.Should().Be(ErrorCodes.Bookings.InvalidState);
    }

    [Fact]
    public async Task HandleAsync_Should_Succeed_When_UserIsSystemAdmin()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var command = new RejectBookingCommand(booking.Id, "Admin Reason", true, null, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Rejected);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnConflict_When_ConcurrencyOccurs()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _bookingRepoMock.Setup(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MeAjudaAi.Shared.Exceptions.ConcurrencyConflictException());

        var command = new RejectBookingCommand(booking.Id, "Reason", false, providerId, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(409);
    }
}
