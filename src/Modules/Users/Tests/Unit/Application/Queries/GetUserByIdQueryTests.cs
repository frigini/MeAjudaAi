using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class GetUserByIdQueryTests
{
    [Fact]
    public void Constructor_WithValidUserId_ShouldCreateQuery()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var query = new GetUserByIdQuery(userId);

        // Assert
        query.Should().NotBeNull();
        query.UserId.Should().Be(userId);
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be($"user:id:{userId}");
    }

    [Fact]
    public void GetCacheKey_WithDifferentUserIds_ShouldReturnDifferentKeys()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var query1 = new GetUserByIdQuery(userId1);
        var query2 = new GetUserByIdQuery(userId2);

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().Be($"user:id:{userId1}");
        key2.Should().Be($"user:id:{userId2}");
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn15Minutes()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.NewGuid());

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().HaveCount(2);
        tags.Should().Contain("users");
        tags.Should().Contain($"user:{userId}");
    }

    [Fact]
    public void GetCacheTags_WithDifferentUserIds_ShouldReturnDifferentUserTags()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var query1 = new GetUserByIdQuery(userId1);
        var query2 = new GetUserByIdQuery(userId2);

        // Act
        var tags1 = query1.GetCacheTags();
        var tags2 = query2.GetCacheTags();

        // Assert
        tags1.Should().Contain($"user:{userId1}");
        tags2.Should().Contain($"user:{userId2}");
        tags1.Should().Contain("users");
        tags2.Should().Contain("users");
    }

    [Fact]
    public void Query_ShouldImplementICacheableQuery()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.NewGuid());

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableQuery>();
    }

    [Fact]
    public void Query_ShouldBeQueryOfResultUserDto()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.NewGuid());

        // Act & Assert
        query.Should().BeAssignableTo<Query<Result<UserDto>>>();
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldCreateQuery()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var query = new GetUserByIdQuery(userId);

        // Assert
        query.Should().NotBeNull();
        query.UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetCacheKey_WithEmptyGuid_ShouldHandleGracefully()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.Empty);

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be($"user:id:{Guid.Empty}");
    }
}
