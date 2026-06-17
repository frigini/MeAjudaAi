using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Persistence;

public class BookingsDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldConfigureModelCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: "BookingsTestDb_" + Guid.NewGuid())
            .Options;
        var domainEventProcessorMock = new Mock<IDomainEventProcessor>();

        // Act
        using var context = new BookingsDbContext(options, domainEventProcessorMock.Object);
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetDefaultSchema().Should().Be("bookings");

        // Check if entities are registered
        model.FindEntityType(typeof(MeAjudaAi.Modules.Bookings.Domain.Entities.Booking)).Should().NotBeNull();
        model.FindEntityType(typeof(MeAjudaAi.Modules.Bookings.Domain.Entities.ProviderSchedule)).Should().NotBeNull();
    }
}
