using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Persistence;

public class DocumentsDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldBuildModel()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act
        using var ctx = new MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext(options);

        // Assert
        ctx.Model.GetEntityTypes().Should().NotBeEmpty();
    }
}
