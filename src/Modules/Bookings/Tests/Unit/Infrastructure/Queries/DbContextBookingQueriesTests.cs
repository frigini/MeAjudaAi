using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Queries;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Infrastructure")]
public class DbContextBookingQueriesTests : BaseSqliteInMemoryDatabaseTest<BookingsDbContext>
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
        var booking = new BookingBuilder()
            .WithProviderId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .WithStatus(EBookingStatus.Pending)
            .Build();
        DbContext.Bookings.Add(booking);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.GetByIdAsync(booking.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenCompletedBookingExists_ShouldReturnTrue()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(clientId)
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(10, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .AsCompleted()
            .Build();
        DbContext.Bookings.Add(booking);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.HasCompletedBookingAsync(clientId, providerId);

        // Assert
        result.Should().BeTrue();
    }
}
