using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Persistence;

public class ProvidersDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldBuildModel()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act
        using var ctx = new ProvidersDbContext(options, null!);

        // Assert
        ctx.Model.GetEntityTypes().Should().NotBeEmpty();
    }
}
