using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Handlers.Commands;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class CancelBookingCommandHandlerTests
{
    private readonly Mock<IBookingQueries> _bookingQueriesMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ILogger<CancelBookingCommandHandler>> _loggerMock = new();
    private readonly CancelBookingCommandHandler _sut;

    public CancelBookingCommandHandlerTests()
    {
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new CancelBookingCommandHandler(
            _bookingQueriesMock.Object,
            _uowMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Cancel_When_UserIsClientOwner()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var booking = new BookingBuilder()
            .WithProviderId(Guid.NewGuid())
            .WithClientId(clientId)
            .WithServiceId(Guid.NewGuid())
            .WithDate(tomorrow)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .Build();
        
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Reason", false, null, clientId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Cancelled);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Cancel_When_OwnerIdentifiedByUserId()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var booking = new BookingBuilder()
            .WithProviderId(Guid.NewGuid())
            .WithClientId(clientId)
            .WithServiceId(Guid.NewGuid())
            .WithDate(tomorrow)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .Build();
        
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act - Simulando o claim vindo do NameIdentifier mapeado para UserClientId no comando
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Reason", false, null, clientId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Cancelled);
    }

    [Fact]
    public async Task HandleAsync_Should_Cancel_When_UserIsProviderOwner()
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

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Provider Reason", false, providerId, null, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Cancelled);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnForbidden_When_UserIsDifferentClient()
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
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Reason", false, null, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnForbidden_When_UserIsDifferentProvider()
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
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Reason", false, Guid.NewGuid(), null, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task HandleAsync_Should_Succeed_When_UserIsSystemAdmin()
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
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Admin Reason", true, null, null, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Cancelled);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnConflict_When_ConcurrencyOccurs()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var booking = new BookingBuilder()
            .WithProviderId(Guid.NewGuid())
            .WithClientId(clientId)
            .WithServiceId(Guid.NewGuid())
            .WithDate(tomorrow)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .Build();
        
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Reason", false, null, clientId, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_BookingNotFound()
    {
        // Arrange
        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(Guid.NewGuid(), "Reason", false, null, null, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnBadRequest_When_DomainThrowsInvalidOperation()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var booking = new BookingBuilder()
            .WithProviderId(Guid.NewGuid())
            .WithClientId(clientId)
            .WithServiceId(Guid.NewGuid())
            .WithDate(tomorrow)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .Build();
        
        // Coloca o booking em um estado que não permite cancelamento (Rejeitado)
        booking.Reject("Some reason");

        _bookingQueriesMock.Setup(x => x.GetByIdTrackedAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Reason", false, null, clientId, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        result.Error.Code.Should().Be(ErrorCodes.Bookings.InvalidState);
    }
}