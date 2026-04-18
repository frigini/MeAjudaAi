using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Persistence;

public class UsersDbContextMappingTests
{
    [Fact]
    public void ModelBuilder_ShouldHaveConfiguredUsersSchemaAndTable()
    {
        // Arrange
        using var context = UsersTestDb.CreateSqlite();
        var model = context.Model;

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
