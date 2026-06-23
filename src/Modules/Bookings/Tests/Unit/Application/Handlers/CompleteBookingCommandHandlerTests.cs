using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Handlers;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

[Trait("Category", "Unit")]
public class CompleteBookingCommandHandlerTests
{
    private readonly Mock<IBookingQueries> _bookingQueriesMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ILogger<CompleteBookingCommandHandler>> _loggerMock = new();
    private readonly CompleteBookingCommandHandler _sut;

    public CompleteBookingCommandHandlerTests()
    {
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new CompleteBookingCommandHandler(
            _bookingQueriesMock.Object,
            _uowMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Complete_When_BookingIsConfirmed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(tomorrow)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .AsConfirmed()
            .Build();
        booking.ClearDomainEvents();

        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, false, providerId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Completed);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnBadRequest_When_BookingStateIsInvalidForCompletion()
    {
        // Arrange
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var booking = new BookingBuilder()
            .WithProviderId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(tomorrow)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .Build();

        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, true, null, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        result.Error.Code.Should().Be(ErrorCodes.Bookings.InvalidState);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_UserIsNotOwner()
    {
        // Arrange
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var booking = new BookingBuilder()
            .WithProviderId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(tomorrow)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .AsConfirmed()
            .Build();
        booking.ClearDomainEvents();

        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, false, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_BookingNotFound()
    {
        // Arrange
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(tomorrow)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .AsConfirmed()
            .Build();
        booking.ClearDomainEvents();

        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, true, null, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Completed);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_409_When_SaveChangesAsync_Throws_DbUpdateConcurrencyException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(tomorrow)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .AsConfirmed()
            .Build();
        booking.ClearDomainEvents();
        
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, false, providerId, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}