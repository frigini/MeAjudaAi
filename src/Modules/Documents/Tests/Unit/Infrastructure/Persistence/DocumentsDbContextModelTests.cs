using Microsoft.EntityFrameworkCore;

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

    [Fact]
    public void GetRepository_WithUnsupportedType_ShouldThrowNotSupportedException()
    {
        var options = new DbContextOptionsBuilder<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var ctx = new MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext(options);

        var act = () => ctx.GetRepository<object, Guid>();

        act.Should().Throw<NotSupportedException>()
            .WithMessage("*is not supported*");
    }
}

