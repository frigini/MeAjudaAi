using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class GetBookingByIdQueryHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IProviderScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<ILogger<GetBookingByIdQueryHandler>> _loggerMock = new();
    private readonly GetBookingByIdQueryHandler _sut;

    private const int ExpectedNotFoundStatusCode = 404;
    private const string ExpectedNotFoundMessage = "Agendamento não encontrado.";

    public GetBookingByIdQueryHandlerTests()
    {
        _sut = new GetBookingByIdQueryHandler(
            _bookingRepoMock.Object,
            _scheduleRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_BookingDto_When_Found_And_Authorized()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var booking = Booking.Create(providerId, clientId, Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.ClearDomainEvents();

        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var schedule = ProviderSchedule.Create(providerId);
        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _sut.HandleAsync(new GetBookingByIdQuery(booking.Id, clientId, null, false, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(booking.Id);
        result.Value.ProviderId.Should().Be(providerId);
        result.Value.ClientId.Should().Be(clientId);
        result.Value.Status.Should().Be(EBookingStatus.Pending);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_BookingDto_When_Authorized_As_Provider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.ClearDomainEvents();

        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderSchedule.Create(providerId));

        // Act
        var result = await _sut.HandleAsync(new GetBookingByIdQuery(booking.Id, null, providerId, false, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(booking.Id);
        result.Value.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_BookingDto_When_Authorized_As_Admin()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderSchedule.Create(providerId));

        // Act
        var result = await _sut.HandleAsync(new GetBookingByIdQuery(booking.Id, null, null, true, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Success_When_Schedule_Is_Null()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null);

        // Act
        var result = await _sut.HandleAsync(new GetBookingByIdQuery(booking.Id, null, null, true, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_NotAuthorized()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var booking = Booking.Create(providerId, clientId, Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.ClearDomainEvents();

        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new GetBookingByIdQuery(booking.Id, Guid.NewGuid(), null, false, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(ExpectedNotFoundStatusCode);
        result.Error.Message.Should().Be(ExpectedNotFoundMessage);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_NotAuthorized_OtherProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var otherProviderId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        booking.ClearDomainEvents();

        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.HandleAsync(new GetBookingByIdQuery(booking.Id, null, otherProviderId, false, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(ExpectedNotFoundStatusCode);
        result.Error.Message.Should().Be(ExpectedNotFoundMessage);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_BookingDoesNotExist()
    {
        // Arrange
        _bookingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await _sut.HandleAsync(new GetBookingByIdQuery(Guid.NewGuid(), null, null, true, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(ExpectedNotFoundStatusCode);
        result.Error.Message.Should().Be(ExpectedNotFoundMessage);
    }
}