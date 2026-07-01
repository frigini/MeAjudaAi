using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Handlers.Commands;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class ConfirmBookingCommandHandlerTests
{
    private readonly Mock<IBookingQueries> _bookingQueriesMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ILogger<ConfirmBookingCommandHandler>> _loggerMock = new();
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock = new();
    private readonly ConfirmBookingCommandHandler _sut;

    public ConfirmBookingCommandHandlerTests()
    {
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new ConfirmBookingCommandHandler(
            _bookingQueriesMock.Object,
            _uowMock.Object,
            _loggerMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Confirm_When_UserIsProviderOwner()
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
            .Build();
        
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var command = new ConfirmBookingCommand(booking.Id, false, providerId, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Confirmed);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnConflict_When_ConcurrencyOccurs()
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
            .Build();
        
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var command = new ConfirmBookingCommand(booking.Id, false, providerId, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
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
            .Build();
        
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var command = new ConfirmBookingCommand(booking.Id, false, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_When_BookingDoesNotExist()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        var command = new ConfirmBookingCommand(bookingId, false, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task HandleAsync_Should_Confirm_When_UserIsSystemAdmin()
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

        var command = new ConfirmBookingCommand(booking.Id, true, null, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Confirmed);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_BookingStateIsNotTransitionable()
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
        
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var command = new ConfirmBookingCommand(booking.Id, false, providerId, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        result.Error.Code.Should().Be(ErrorCodes.Bookings.InvalidState);
    }
}