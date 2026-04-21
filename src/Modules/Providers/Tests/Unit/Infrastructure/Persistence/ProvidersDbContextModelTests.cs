using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Persistence;

public class ProvidersDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldBuildModel()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act
        using var ctx = new MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext(options);

        // Assert
        ctx.Model.GetEntityTypes().Should().NotBeEmpty();
    }
}
