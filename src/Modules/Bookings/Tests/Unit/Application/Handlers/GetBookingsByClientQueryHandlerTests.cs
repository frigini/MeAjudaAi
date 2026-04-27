using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class GetBookingsByClientQueryHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IProviderScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<ILogger<GetBookingsByClientQueryHandler>> _loggerMock = new();
    private readonly GetBookingsByClientQueryHandler _sut;

    public GetBookingsByClientQueryHandlerTests()
    {
        _sut = new GetBookingsByClientQueryHandler(
            _bookingRepoMock.Object,
            _scheduleRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_BookingsForClient()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var date = new DateOnly(2026, 4, 25);

        var bookings = new List<Booking>
        {
            Booking.Create(providerId, clientId, Guid.NewGuid(), date,
                TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))),
            Booking.Create(providerId, clientId, Guid.NewGuid(), date,
                TimeSlot.Create(new TimeOnly(14, 0), new TimeOnly(15, 0)))
        };
        bookings.ForEach(b => b.ClearDomainEvents());

        _bookingRepoMock.Setup(x => x.GetByClientIdPagedAsync(clientId, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bookings.AsReadOnly(), 2));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderSchedule.Create(providerId));

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByClientQuery(clientId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().AllSatisfy(b => b.ClientId.Should().Be(clientId));
    }

    [Fact]
    public async Task HandleAsync_Should_Return_EmptyList_When_NoBookings()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _bookingRepoMock.Setup(x => x.GetByClientIdPagedAsync(clientId, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Booking>().AsReadOnly(), 0));

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByClientQuery(clientId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Correct_Pagination_Metadata()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var query = new GetBookingsByClientQuery(clientId, Guid.NewGuid())
        {
            Page = 2,
            PageSize = 5
        };

        _bookingRepoMock.Setup(x => x.GetByClientIdPagedAsync(clientId, null, null, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Booking>().AsReadOnly(), 15));

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.TotalItems.Should().Be(15);
    }

    [Fact]
    public async Task HandleAsync_Should_Apply_Date_Filters()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 30);
        var query = new GetBookingsByClientQuery(clientId, Guid.NewGuid())
        {
            From = from,
            To = to
        };

        _bookingRepoMock.Setup(x => x.GetByClientIdPagedAsync(
                clientId,
                DateOnly.FromDateTime(from),
                DateOnly.FromDateTime(to),
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Booking>().AsReadOnly(), 0));

        // Act
        await _sut.HandleAsync(query);

        // Assert
        _bookingRepoMock.Verify(x => x.GetByClientIdPagedAsync(
            clientId,
            DateOnly.FromDateTime(from),
            DateOnly.FromDateTime(to),
            1,
            10,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Cache_ProviderSchedule_Per_Provider()
    {
        // Arrange — dois bookings do mesmo prestador: repositório de agenda deve ser chamado apenas 1x
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var date = new DateOnly(2026, 5, 1);

        var bookings = new List<Booking>
        {
            Booking.Create(providerId, clientId, Guid.NewGuid(), date,
                TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))),
            Booking.Create(providerId, clientId, Guid.NewGuid(), date,
                TimeSlot.Create(new TimeOnly(11, 0), new TimeOnly(12, 0)))
        };
        bookings.ForEach(b => b.ClearDomainEvents());

        _bookingRepoMock.Setup(x => x.GetByClientIdPagedAsync(clientId, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bookings.AsReadOnly(), 2));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderSchedule.Create(providerId));

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByClientQuery(clientId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        // O schedule do prestador deve ser buscado apenas uma vez (cache interno)
        _scheduleRepoMock.Verify(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
