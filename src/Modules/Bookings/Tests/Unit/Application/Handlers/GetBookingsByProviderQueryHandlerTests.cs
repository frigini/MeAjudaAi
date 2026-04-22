using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class GetBookingsByProviderQueryHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IProviderScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<ILogger<GetBookingsByProviderQueryHandler>> _loggerMock = new();
    private readonly GetBookingsByProviderQueryHandler _sut;

    public GetBookingsByProviderQueryHandlerTests()
    {
        _sut = new GetBookingsByProviderQueryHandler(
            _bookingRepoMock.Object,
            _scheduleRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_BookingsForProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 25);

        var bookings = new List<Booking>
        {
            Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
                TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))),
            Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
                TimeSlot.Create(new TimeOnly(14, 0), new TimeOnly(15, 0)))
        };
        bookings.ForEach(b => b.ClearDomainEvents());

        _bookingRepoMock.Setup(x => x.GetByProviderIdPagedAsync(providerId, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bookings.AsReadOnly(), 2));

        var schedule = ProviderSchedule.Create(providerId);
        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByProviderQuery(providerId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(b => b.ProviderId.Should().Be(providerId));
    }

    [Fact]
    public async Task HandleAsync_Should_Return_EmptyList_When_NoBookings()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _bookingRepoMock.Setup(x => x.GetByProviderIdPagedAsync(providerId, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Booking>().AsReadOnly(), 0));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null);

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByProviderQuery(providerId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_Should_Apply_Filters_And_Pagination()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddDays(7);
        var query = new GetBookingsByProviderQuery(providerId, Guid.NewGuid())
        {
            From = from,
            To = to,
            Page = 2,
            PageSize = 20
        };

        _bookingRepoMock.Setup(x => x.GetByProviderIdPagedAsync(
                providerId,
                DateOnly.FromDateTime(from),
                DateOnly.FromDateTime(to),
                2,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Booking>().AsReadOnly(), 0));

        // Act
        await _sut.HandleAsync(query);

        // Assert
        _bookingRepoMock.Verify(x => x.GetByProviderIdPagedAsync(
            providerId,
            DateOnly.FromDateTime(from),
            DateOnly.FromDateTime(to),
            2,
            20,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Use_Provider_TimeZone()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 25);
        var startTime = new TimeOnly(10, 0);
        var endTime = new TimeOnly(11, 0);
        
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(startTime, endTime));
        
        _bookingRepoMock.Setup(x => x.GetByProviderIdPagedAsync(providerId, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Booking> { booking }.AsReadOnly(), 1));

        // Tokyo is UTC+9
        var schedule = ProviderSchedule.Create(providerId, "Tokyo Standard Time");
        
        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByProviderQuery(providerId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value?.First();
        
        dto.Should().NotBeNull();
        dto!.Start.Hour.Should().Be(10);
        dto!.End.Hour.Should().Be(11);

        // Verificando o UTC para Tokyo (UTC+9): 10:00 local -> 01:00 UTC
        dto!.Start.UtcDateTime.Hour.Should().Be(1);
        dto!.End.UtcDateTime.Hour.Should().Be(2);
        dto!.Start.Offset.Should().Be(TimeSpan.FromHours(9));
    }

    [Fact]
    public async Task HandleAsync_Should_Use_Fallback_TimeZone_When_ScheduleNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 4, 25),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _bookingRepoMock.Setup(x => x.GetByProviderIdPagedAsync(providerId, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Booking> { booking }.AsReadOnly(), 1));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null);

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByProviderQuery(providerId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
}
