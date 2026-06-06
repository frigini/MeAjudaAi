using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Infrastructure")]
public class DbContextProviderScheduleQueriesTests : IDisposable
{
    private readonly BookingsDbContext _dbContext;
    private readonly DbContextProviderScheduleQueries _queries;

    public DbContextProviderScheduleQueriesTests()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase("ProviderScheduleQueriesTest_" + Guid.NewGuid())
            .Options;
        _dbContext = new BookingsDbContext(options);
        _queries = new DbContextProviderScheduleQueries(_dbContext);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task GetByProviderIdAsync_WithExistingSchedule_ShouldReturnSchedule()
    {
        var providerId = Guid.NewGuid();
        var schedule = ProviderSchedule.Create(providerId);
        _dbContext.ProviderSchedules.Add(schedule);
        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetByProviderIdAsync(providerId);

        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        var result = await _queries.GetByProviderIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }
}
