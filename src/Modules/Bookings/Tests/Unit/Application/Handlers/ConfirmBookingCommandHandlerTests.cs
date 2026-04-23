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

public class ConfirmBookingCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();
    private readonly Mock<ILogger<ConfirmBookingCommandHandler>> _loggerMock = new();
    private readonly ConfirmBookingCommandHandler _sut;

    public ConfirmBookingCommandHandlerTests()
    {
        _sut = new ConfirmBookingCommandHandler(
            _bookingRepoMock.Object,
            _httpContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Confirm_When_UserIsProviderOwner()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 22);
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        SetupUser(providerId);

        // Act
        var result = await _sut.HandleAsync(new ConfirmBookingCommand(booking.Id, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(Contracts.Bookings.Enums.EBookingStatus.Confirmed);
        _bookingRepoMock.Verify(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_UserIsNotOwner()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 22);
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        SetupUser(Guid.NewGuid()); // Outro provider

        // Act
        var result = await _sut.HandleAsync(new ConfirmBookingCommand(booking.Id, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(403);
        _bookingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_When_BookingDoesNotExist()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        SetupUser(Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(new ConfirmBookingCommand(bookingId, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_Should_RequireProviderClaim_When_UserHasNoProviderId()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 4, 22),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "Test")) };
        _httpContextMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = await _sut.HandleAsync(new ConfirmBookingCommand(booking.Id, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_BookingStateIsNotTransitionable()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 4, 22),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.Confirm(); // Já confirmado, não pode confirmar novamente
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        SetupUser(providerId);

        // Act
        var result = await _sut.HandleAsync(new ConfirmBookingCommand(booking.Id, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
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
