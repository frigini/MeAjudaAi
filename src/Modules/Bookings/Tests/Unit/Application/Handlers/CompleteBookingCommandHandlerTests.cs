using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class CompleteBookingCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();
    private readonly Mock<ILogger<CompleteBookingCommandHandler>> _loggerMock = new();
    private readonly CompleteBookingCommandHandler _sut;

    public CompleteBookingCommandHandlerTests()
    {
        _sut = new CompleteBookingCommandHandler(
            _bookingRepoMock.Object,
            _httpContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Complete_When_BookingIsConfirmed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 25);
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.Confirm();
        booking.ClearDomainEvents();
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        SetupUser(providerId);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(EBookingStatus.Completed);
        _bookingRepoMock.Verify(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_BookingIsPending()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 25);
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.ClearDomainEvents();
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        SetupUser(providerId);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
        _bookingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_UserIsNotOwner()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 25);
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.Confirm();
        booking.ClearDomainEvents();
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        SetupUser(Guid.NewGuid()); // Outro provider

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(booking.Id, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(403);
        _bookingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_BookingNotFound()
    {
        // Arrange
        SetupUser(Guid.NewGuid());
        _bookingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await _sut.HandleAsync(new CompleteBookingCommand(Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
        _bookingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupUser(Guid providerId)
    {
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.ProviderId, providerId.ToString()),
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };
        _httpContextMock.Setup(x => x.HttpContext).Returns(context);
    }
}
