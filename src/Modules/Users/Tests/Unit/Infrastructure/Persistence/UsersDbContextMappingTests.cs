using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Persistence;

public class UsersDbContextMappingTests : BaseSqliteInMemoryDatabaseTest<UsersDbContext>
{
    public UsersDbContextMappingTests()
        : base(options => new UsersDbContext(options))
    {
    }

    [Fact]
    public void ModelBuilder_ShouldHaveConfiguredUsersSchemaAndTable()
    {
        // Arrange
        var model = DbContext.Model;

        // Act
        var userEntity = model.FindEntityType("MeAjudaAi.Modules.Users.Domain.Entities.User");

        // Assert
        userEntity.Should().NotBeNull();
        userEntity!.GetSchema().Should().Be("users");
        userEntity.GetTableName().Should().Be("users");

        // Key verification
        var primaryKey = userEntity.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }
}
