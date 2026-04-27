using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class CreateBookingCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IProviderScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<IProvidersModuleApi> _providersApiMock = new();
    private readonly Mock<IServiceCatalogsModuleApi> _serviceCatalogsApiMock = new();
    private readonly Mock<ILogger<CreateBookingCommandHandler>> _loggerMock = new();
    private readonly CreateBookingCommandHandler _sut;

    public CreateBookingCommandHandlerTests()
    {
        _sut = new CreateBookingCommandHandler(
            _bookingRepoMock.Object,
            _scheduleRepoMock.Object,
            _providersApiMock.Object,
            _serviceCatalogsApiMock.Object,
            _loggerMock.Object);
    }

    private CreateBookingCommand BuildFutureCommand(Guid? providerId = null, Guid? serviceId = null, int daysOffset = 2, int hour = 10)
    {
        var start = DateTimeOffset.UtcNow.Date.AddDays(daysOffset).AddHours(hour);
        return new CreateBookingCommand(
            providerId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            serviceId ?? Guid.NewGuid(),
            new DateTimeOffset(start, TimeSpan.Zero),
            new DateTimeOffset(start.AddHours(1), TimeSpan.Zero),
            Guid.NewGuid());
    }

    private void SetupHappyPath(Guid providerId, Guid serviceId, ProviderSchedule? schedule = null)
    {
        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _providersApiMock.Setup(x => x.IsServiceOfferedByProviderAsync(providerId, serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        if (schedule != null)
        {
            _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedule);
        }

        _bookingRepoMock.Setup(x => x.AddIfNoOverlapAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    [Fact]
    public async Task HandleAsync_Should_CreateBooking_When_Valid()
    {
        // Arrange
        var command = BuildFutureCommand();
        var schedule = ProviderSchedule.Create(command.ProviderId, "UTC");
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0))]));
        
        SetupHappyPath(command.ProviderId, command.ServiceId, schedule);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_Should_Call_AddIfNoOverlapAsync_Once()
    {
        // Arrange
        var command = BuildFutureCommand();
        var schedule = ProviderSchedule.Create(command.ProviderId, "UTC");
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0))]));
        
        SetupHappyPath(command.ProviderId, command.ServiceId, schedule);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _bookingRepoMock.Verify(x => x.AddIfNoOverlapAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProviderNotFound()
    {
        // Arrange
        var command = BuildFutureCommand();
        _providersApiMock.Setup(x => x.ProviderExistsAsync(command.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
        result.Error.Code.Should().Be(ErrorCodes.Providers.ProviderNotFound);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_EndBeforeStart()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);
        var command = new CreateBookingCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            start, start.AddHours(-1), Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Code.Should().Be(ErrorCodes.Bookings.InvalidTime);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_StartInPast()
    {
        // Arrange
        var past = DateTimeOffset.UtcNow.AddHours(-1);
        var command = new CreateBookingCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            past, past.AddHours(1), Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Code.Should().Be(ErrorCodes.Bookings.StartNotInFuture);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProviderHasNoSchedule()
    {
        // Arrange
        var command = BuildFutureCommand();
        _providersApiMock.Setup(x => x.ProviderExistsAsync(command.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(command.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        _providersApiMock.Setup(x => x.IsServiceOfferedByProviderAsync(command.ProviderId, command.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(command.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Code.Should().Be(ErrorCodes.Providers.ScheduleNotFound);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProviderIsUnavailable()
    {
        // Arrange
        var command = BuildFutureCommand();
        _providersApiMock.Setup(x => x.ProviderExistsAsync(command.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(command.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        _providersApiMock.Setup(x => x.IsServiceOfferedByProviderAsync(command.ProviderId, command.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var schedule = ProviderSchedule.Create(command.ProviderId, "UTC");
        // Disponibilidade apenas na parte da tarde
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(14, 0), new TimeOnly(18, 0))]));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(command.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Code.Should().Be(ErrorCodes.Providers.ProviderUnavailable);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_OverlapDetectedByRepo()
    {
        // Arrange
        var command = BuildFutureCommand();
        var schedule = ProviderSchedule.Create(command.ProviderId, "UTC");
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0))]));
        
        SetupHappyPath(command.ProviderId, command.ServiceId, schedule);

        _bookingRepoMock.Setup(x => x.AddIfNoOverlapAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.Conflict("Overlap", ErrorCodes.Bookings.Overlap)));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(409);
        result.Error.Code.Should().Be(ErrorCodes.Bookings.Overlap);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ServiceNotOfferedByProvider()
    {
        // Arrange
        var command = BuildFutureCommand();
        _providersApiMock.Setup(x => x.ProviderExistsAsync(command.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(command.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var schedule = ProviderSchedule.Create(command.ProviderId, "UTC");
        // Dá disponibilidade válida para que o erro venha exclusivamente de "ServiceNotOffered"
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0))]));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(command.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        _providersApiMock.Setup(x => x.IsServiceOfferedByProviderAsync(command.ProviderId, command.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
        result.Error.Code.Should().Be(ErrorCodes.Providers.ServiceNotOffered);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProvidersApiFails()
    {
        // Arrange
        var command = BuildFutureCommand();
        _providersApiMock.Setup(x => x.ProviderExistsAsync(command.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure(new Error("API Error", 500, ErrorCodes.InternalError)));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCodes.InternalError);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ServiceCatalogsApiFails()
    {
        // Arrange
        var command = BuildFutureCommand();
        _providersApiMock.Setup(x => x.ProviderExistsAsync(command.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(command.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure(new Error("Catalog Error", 500, ErrorCodes.InternalError)));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCodes.InternalError);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_StartIsNotInFuture()
    {
        // Arrange
        // Pequeno recuo de 10ms para garantir que o start NUNCA estará no futuro 
        // em relação ao UtcNow lido dentro do handler
        var pastStart = DateTimeOffset.UtcNow.AddMilliseconds(-10);
        var command = new CreateBookingCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            pastStart, pastStart.AddHours(1), Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCodes.Bookings.StartNotInFuture);
    }
}
