using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Queries;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Infrastructure")]
public class DbContextBookingQueriesTests : BaseInMemoryDatabaseTest<BookingsDbContext>
{
    private readonly DbContextBookingQueries _queries;

    public DbContextBookingQueriesTests() : base(options => new BookingsDbContext(options))
    {
        _queries = new DbContextBookingQueries(DbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingBooking_ShouldReturnBooking()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var booking = new Booking(bookingId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)), 
            EBookingStatus.Pending, 1);
        DbContext.Bookings.Add(booking);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.GetByIdAsync(bookingId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(bookingId);
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenCompletedBookingExists_ShouldReturnTrue()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var booking = new Booking(Guid.NewGuid(), providerId, clientId, Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)), 
            EBookingStatus.Completed, 1);
        DbContext.Bookings.Add(booking);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.HasCompletedBookingAsync(clientId, providerId);

        // Assert
        result.Should().BeTrue();
    }
}
