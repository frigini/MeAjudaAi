using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Infrastructure")]
public class DbContextBookingCommandServiceTests : BaseInMemoryDatabaseTest<BookingsDbContext>
{
    private readonly Mock<ILogger<DbContextBookingCommandService>> _loggerMock = new();
    private readonly DbContextBookingCommandService _service;

    public DbContextBookingCommandServiceTests() : base(options => new BookingsDbContext(options))
    {
        _service = new DbContextBookingCommandService(DbContext, _loggerMock.Object);
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_AddBooking_When_NoOverlapExists()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0)));

        // Act
        var result = await _service.AddIfNoOverlapAsync(booking);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var savedBooking = await DbContext.Bookings.FindAsync(booking.Id);
        savedBooking.Should().NotBeNull();
    }
}
