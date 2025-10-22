using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Persistence;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Infrastructure")]
public class UserConfigurationTests
{
    [Fact]
    public void UserConfiguration_ShouldImplementIEntityTypeConfiguration()
    {
        // Arrange & Act
        var configurationType = typeof(UserConfiguration);

        // Assert
        configurationType.Should().Implement<IEntityTypeConfiguration<User>>();
    }

    [Fact]
    public void UserConfiguration_ShouldHaveParameterlessConstructor()
    {
        // Arrange & Act
        var constructors = typeof(UserConfiguration).GetConstructors();

        // Assert
        constructors.Should().HaveCount(1);
        constructors[0].GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void Configure_ShouldNotThrowException()
    {
        // Arrange
        var configuration = new UserConfiguration();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act & Assert
        var act = () =>
        {
            using var context = new TestDbContext(options);
            var entityType = context.Model.FindEntityType(typeof(User));
            entityType.Should().NotBeNull();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void UserConfiguration_ShouldConfigureTableName()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act
        using var context = new TestDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(User));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("users");
    }

    [Fact]
    public void UserConfiguration_ShouldConfigurePrimaryKey()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act
        using var context = new TestDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(User));

        // Assert
        entityType.Should().NotBeNull();
        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    // Test DbContext para uso nos testes
    private class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<User>? Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
