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
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class CancelBookingCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();
    private readonly Mock<ILogger<CancelBookingCommandHandler>> _loggerMock = new();
    private readonly CancelBookingCommandHandler _sut;

    public CancelBookingCommandHandlerTests()
    {
        _sut = new CancelBookingCommandHandler(
            _bookingRepoMock.Object,
            _httpContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Cancel_When_UserIsClientOwner()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var booking = Booking.Create(Guid.NewGuid(), clientId, Guid.NewGuid(), new DateOnly(2026, 4, 22),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        SetupUser(clientId, null);

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Reason", Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(Contracts.Bookings.Enums.EBookingStatus.Cancelled);
        _bookingRepoMock.Verify(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_UserIsNotAuthorized()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 4, 22),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        SetupUser(Guid.NewGuid(), null); // Random user

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Reason", Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task HandleAsync_Should_Succeed_When_UserIsSystemAdmin()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 4, 22),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Setup Admin
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Subject, Guid.NewGuid().ToString()),
            new(AuthConstants.Claims.IsSystemAdmin, "true")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _httpContextMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = principal });

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Admin Reason", Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(Contracts.Bookings.Enums.EBookingStatus.Cancelled);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnConflict_When_ConcurrencyOccurs()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var booking = Booking.Create(Guid.NewGuid(), clientId, Guid.NewGuid(), new DateOnly(2026, 4, 22),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _bookingRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MeAjudaAi.Shared.Exceptions.ConcurrencyConflictException());

        SetupUser(clientId, null);

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(booking.Id, "Reason", Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnUnauthorized_When_UserNotAuthenticated()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext()); // No User

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(Guid.NewGuid(), "Reason", Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_BookingNotFound()
    {
        // Arrange
        _bookingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);
        SetupUser(Guid.NewGuid(), null);

        // Act
        var result = await _sut.HandleAsync(new CancelBookingCommand(Guid.NewGuid(), "Reason", Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }

    private void SetupUser(Guid userId, Guid? providerId)
    {
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Subject, userId.ToString())
        };

        if (providerId.HasValue)
        {
            claims.Add(new Claim(AuthConstants.Claims.ProviderId, providerId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };
        _httpContextMock.Setup(x => x.HttpContext).Returns(context);
    }
}
