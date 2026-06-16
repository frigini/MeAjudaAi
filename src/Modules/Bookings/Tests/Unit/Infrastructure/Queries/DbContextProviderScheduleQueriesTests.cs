using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Queries;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Infrastructure")]
public class DbContextProviderScheduleQueriesTests : BaseInMemoryDatabaseTest<BookingsDbContext>
{
    private readonly DbContextProviderScheduleQueries _queries;

    public DbContextProviderScheduleQueriesTests() : base(options => new BookingsDbContext(options))
    {
        _queries = new DbContextProviderScheduleQueries(DbContext);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithExistingSchedule_ShouldReturnSchedule()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var schedule = new ProviderScheduleBuilder()
            .WithProviderId(providerId)
            .Build();
        DbContext.ProviderSchedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.GetByProviderIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        // Act
        var result = await _queries.GetByProviderIdAsync(providerId);

        // Assert
        result.Should().BeNull();
    }
}