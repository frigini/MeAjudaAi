using MeAjudaAi.Modules.Bookings.Application.Handlers;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class GetBookingsByClientQueryHandlerTests : BaseUnitTest
{
    private readonly Mock<IBookingQueries> _bookingQueriesMock = new();
    private readonly Mock<IProviderScheduleQueries> _scheduleQueriesMock = new();
    private readonly Mock<ILogger<GetBookingsByClientQueryHandler>> _loggerMock = new();
    private readonly GetBookingsByClientQueryHandler _sut;

    public GetBookingsByClientQueryHandlerTests()
    {
        _sut = new GetBookingsByClientQueryHandler(
            _bookingQueriesMock.Object,
            _scheduleQueriesMock.Object,
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
            new BookingBuilder()
                .WithProviderId(providerId)
                .WithClientId(clientId)
                .WithServiceId(Guid.NewGuid())
                .WithDate(date)
                .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
                .Build(),
            new BookingBuilder()
                .WithProviderId(providerId)
                .WithClientId(clientId)
                .WithServiceId(Guid.NewGuid())
                .WithDate(date)
                .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(14, 0)).WithEnd(new TimeOnly(15, 0)).Build())
                .Build()
        };
        bookings.ForEach(b => b.ClearDomainEvents());

        _bookingQueriesMock.Setup(x => x.GetByClientIdPagedAsync(clientId, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bookings.AsReadOnly(), 2));

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProviderScheduleBuilder().WithProviderId(providerId).Build());

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByClientQuery(clientId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.Items.Should().AllSatisfy(b => b.ClientId.Should().Be(clientId));
    }

    [Fact]
    public async Task HandleAsync_Should_Return_EmptyList_When_NoBookings()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _bookingQueriesMock.Setup(x => x.GetByClientIdPagedAsync(clientId, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Booking>().AsReadOnly(), 0));

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByClientQuery(clientId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
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

        _bookingQueriesMock.Setup(x => x.GetByClientIdPagedAsync(clientId, null, null, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Booking>().AsReadOnly(), 15));

        // Act
        var result = await _sut.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.PageNumber.Should().Be(2);
        result.Value!.PageSize.Should().Be(5);
        result.Value!.TotalItems.Should().Be(15);
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

        _bookingQueriesMock.Setup(x => x.GetByClientIdPagedAsync(
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
        _bookingQueriesMock.Verify(x => x.GetByClientIdPagedAsync(
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
            new BookingBuilder()
                .WithProviderId(providerId)
                .WithClientId(clientId)
                .WithServiceId(Guid.NewGuid())
                .WithDate(date)
                .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(9, 0)).WithEnd(new TimeOnly(10, 0)).Build())
                .Build(),
            new BookingBuilder()
                .WithProviderId(providerId)
                .WithClientId(clientId)
                .WithServiceId(Guid.NewGuid())
                .WithDate(date)
                .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(11, 0)).WithEnd(new TimeOnly(12, 0)).Build())
                .Build()
        };
        bookings.ForEach(b => b.ClearDomainEvents());

        _bookingQueriesMock.Setup(x => x.GetByClientIdPagedAsync(clientId, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bookings.AsReadOnly(), 2));

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProviderScheduleBuilder().WithProviderId(providerId).Build());

        // Act
        var result = await _sut.HandleAsync(new GetBookingsByClientQuery(clientId, Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        // O schedule do prestador deve ser buscado apenas uma vez (cache interno)
        _scheduleQueriesMock.Verify(x => x.GetByProviderIdReadOnlyAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
    }
}