using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class CreateBookingCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IProviderScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<IProvidersModuleApi> _providersApiMock = new();
    private readonly Mock<ILogger<CreateBookingCommandHandler>> _loggerMock = new();
    private readonly CreateBookingCommandHandler _sut;

    public CreateBookingCommandHandlerTests()
    {
        _sut = new CreateBookingCommandHandler(
            _bookingRepoMock.Object,
            _scheduleRepoMock.Object,
            _providersApiMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_CreateBooking_When_Valid()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(10),
            DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(11));

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var schedule = ProviderSchedule.Create(providerId);
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(command.Start.Date.AddHours(8), command.Start.Date.AddHours(18))]));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        _bookingRepoMock.Setup(x => x.AddIfNoOverlapAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _bookingRepoMock.Verify(x => x.AddIfNoOverlapAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_EndBeforeStart()
    {
        // Arrange
        var command = new CreateBookingCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(2),
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProviderNotFound()
    {
        // Arrange
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));
        _providersApiMock.Setup(x => x.ProviderExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }
}
