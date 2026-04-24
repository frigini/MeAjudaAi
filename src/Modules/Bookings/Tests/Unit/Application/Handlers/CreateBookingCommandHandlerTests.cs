using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
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

        // Mock padrão para evitar quebra de testes legados
        _providersApiMock.Setup(x => x.IsServiceOfferedByProviderAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
    }

    [Fact]
    public async Task HandleAsync_Should_CreateBooking_When_Valid()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseUtc = DateTimeOffset.UtcNow.Date;
        var start = baseUtc.AddDays(2).AddHours(10); 
        var end = start.AddHours(1);
        
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), Guid.NewGuid(),
            new DateTimeOffset(start, TimeSpan.Zero), 
            new DateTimeOffset(end, TimeSpan.Zero), 
            Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var schedule = ProviderSchedule.Create(providerId, "UTC");
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0))]));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        _bookingRepoMock.Setup(x => x.AddIfNoOverlapAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_Should_Call_AddIfNoOverlapAsync_Once()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseUtc = DateTimeOffset.UtcNow.Date;
        var day1Start = baseUtc.AddDays(1).AddHours(10);
        
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), Guid.NewGuid(),
            new DateTimeOffset(day1Start, TimeSpan.Zero), 
            new DateTimeOffset(day1Start.AddHours(1), TimeSpan.Zero), 
            Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var schedule = ProviderSchedule.Create(providerId, "UTC");
        schedule.SetAvailability(Availability.Create(day1Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0))]));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
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
    public async Task HandleAsync_Should_Fail_When_ProviderNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseUtc = DateTimeOffset.UtcNow.Date;
        var start = baseUtc.AddDays(1).AddHours(10);
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), Guid.NewGuid(),
            new DateTimeOffset(start, TimeSpan.Zero), 
            new DateTimeOffset(start.AddHours(1), TimeSpan.Zero), 
            Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_EndBeforeStart()
    {
        // Arrange
        var baseUtc = DateTimeOffset.UtcNow.Date;
        var start = baseUtc.AddDays(1).AddHours(10);
        var command = new CreateBookingCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new DateTimeOffset(start, TimeSpan.Zero), 
            new DateTimeOffset(start.AddHours(-1), TimeSpan.Zero), 
            Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
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
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProviderHasNoSchedule()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseUtc = DateTimeOffset.UtcNow.Date;
        var start = baseUtc.AddDays(1).AddHours(10);
        
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), Guid.NewGuid(),
            new DateTimeOffset(start, TimeSpan.Zero), 
            new DateTimeOffset(start.AddHours(1), TimeSpan.Zero), 
            Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProviderIsUnavailable()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseUtc = DateTimeOffset.UtcNow.Date;
        var start = baseUtc.AddDays(1).AddHours(10);
        
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), Guid.NewGuid(),
            new DateTimeOffset(start, TimeSpan.Zero), 
            new DateTimeOffset(start.AddHours(1), TimeSpan.Zero), 
            Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var schedule = ProviderSchedule.Create(providerId, "UTC");
        // Disponibilidade apenas na parte da tarde
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(14, 0), new TimeOnly(18, 0))]));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_OverlapDetectedByRepo()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseUtc = DateTimeOffset.UtcNow.Date;
        var start = baseUtc.AddDays(1).AddHours(10);
        
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), Guid.NewGuid(),
            new DateTimeOffset(start, TimeSpan.Zero), 
            new DateTimeOffset(start.AddHours(1), TimeSpan.Zero), 
            Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var schedule = ProviderSchedule.Create(providerId, "UTC");
        schedule.SetAvailability(Availability.Create(command.Start.DayOfWeek, 
            [TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0))]));
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        _bookingRepoMock.Setup(x => x.AddIfNoOverlapAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.Conflict("Overlap")));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ServiceNotOfferedByProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var baseUtc = DateTimeOffset.UtcNow.Date;
        var start = baseUtc.AddDays(1).AddHours(10);
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), serviceId,
            new DateTimeOffset(start, TimeSpan.Zero), 
            new DateTimeOffset(start.AddHours(1), TimeSpan.Zero), 
            Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _providersApiMock.Setup(x => x.IsServiceOfferedByProviderAsync(providerId, serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
        result.Error.Message.Should().Contain("não oferecido");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProvidersApiFails()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(1), Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure(Error.Internal("API Error")));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("API Error");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ServiceCatalogsApiFails()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var command = new CreateBookingCommand(
            providerId, Guid.NewGuid(), serviceId,
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(1), Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _serviceCatalogsApiMock.Setup(x => x.IsServiceActiveAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure(Error.Internal("Catalog Error")));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Catalog Error");
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
        result.Error!.Message.Should().Contain("no futuro");
    }
}
