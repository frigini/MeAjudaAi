using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

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
        var start = new DateTimeOffset(new DateTime(2026, 4, 22, 10, 0, 0), TimeSpan.Zero);
        var end = new DateTimeOffset(new DateTime(2026, 4, 22, 11, 0, 0), TimeSpan.Zero);
        
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), Guid.NewGuid(),
            start, end, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var schedule = ProviderSchedule.Create(providerId, "UTC");
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0))]));
        
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
        var start = new DateTimeOffset(new DateTime(2026, 4, 22, 11, 0, 0), TimeSpan.Zero);
        var end = new DateTimeOffset(new DateTime(2026, 4, 22, 10, 0, 0), TimeSpan.Zero);

        var command = new CreateBookingCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            start, end, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
        result.Error!.Message.Should().Contain("término deve ser após");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_StartIsPast()
    {
        // Arrange
        var pastStart = DateTimeOffset.UtcNow.AddHours(-1);
        var end = pastStart.AddHours(1);

        var command = new CreateBookingCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            pastStart, end, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
        result.Error!.Message.Should().Contain("horário de início deve ser no futuro");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProviderHasNoSchedule()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var start = new DateTimeOffset(new DateTime(2026, 4, 22, 10, 0, 0), TimeSpan.Zero);
        var end = new DateTimeOffset(new DateTime(2026, 4, 22, 11, 0, 0), TimeSpan.Zero);
        
        var command = new CreateBookingCommand(providerId, Guid.NewGuid(), Guid.NewGuid(), start, end, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
        result.Error!.Message.Should().Contain("não possui agenda configurada");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProviderIsUnavailable()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var start = new DateTimeOffset(new DateTime(2026, 4, 22, 10, 0, 0), TimeSpan.Zero);
        var end = new DateTimeOffset(new DateTime(2026, 4, 22, 11, 0, 0), TimeSpan.Zero);
        
        var command = new CreateBookingCommand(providerId, Guid.NewGuid(), Guid.NewGuid(), start, end, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var schedule = ProviderSchedule.Create(providerId, "UTC");
        // Disponibilidade apenas das 14:00 às 18:00
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(14, 0), new TimeOnly(18, 0))]));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
        result.Error!.Message.Should().Contain("indisponível");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_OverlapDetectedByRepo()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var start = new DateTimeOffset(new DateTime(2026, 4, 22, 10, 0, 0), TimeSpan.Zero);
        var end = new DateTimeOffset(new DateTime(2026, 4, 22, 11, 0, 0), TimeSpan.Zero);
        
        var command = new CreateBookingCommand(providerId, Guid.NewGuid(), Guid.NewGuid(), start, end, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var schedule = ProviderSchedule.Create(providerId, "UTC");
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0))]));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        _bookingRepoMock.Setup(x => x.AddIfNoOverlapAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.Conflict("Overlap")));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(409);
    }
}
