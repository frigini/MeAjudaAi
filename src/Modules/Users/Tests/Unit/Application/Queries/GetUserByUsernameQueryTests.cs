using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class GetUserByUsernameQueryTests
{
    [Fact]
    public void Constructor_WithValidUsername_ShouldCreateQuery()
    {
        // Arrange
        var username = "testuser";

        // Act
        var query = new GetUserByUsernameQuery(username);

        // Assert
        query.Should().NotBeNull();
        query.Username.Should().Be(username);
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var username = "TestUser";
        var query = new GetUserByUsernameQuery(username);

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be("user:username:testuser");
    }

    [Fact]
    public void GetCacheKey_WithDifferentUsernames_ShouldReturnDifferentKeys()
    {
        // Arrange
        var query1 = new GetUserByUsernameQuery("user1");
        var query2 = new GetUserByUsernameQuery("user2");

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().Be("user:username:user1");
        key2.Should().Be("user:username:user2");
    }

    [Fact]
    public void GetCacheKey_WithMixedCase_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var query1 = new GetUserByUsernameQuery("TestUser");
        var query2 = new GetUserByUsernameQuery("TESTUSER");
        var query3 = new GetUserByUsernameQuery("testuser");

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();
        var key3 = query3.GetCacheKey();

        // Assert
        key1.Should().Be("user:username:testuser");
        key2.Should().Be("user:username:testuser");
        key3.Should().Be("user:username:testuser");
        key1.Should().Be(key2).And.Be(key3);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn15Minutes()
    {
        // Arrange
        var query = new GetUserByUsernameQuery("testuser");

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var username = "TestUser";
        var query = new GetUserByUsernameQuery(username);

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().HaveCount(2);
        tags.Should().Contain("users");
        tags.Should().Contain("user-username:testuser");
    }

    [Fact]
    public void GetCacheTags_WithDifferentUsernames_ShouldReturnDifferentUserTags()
    {
        // Arrange
        var query1 = new GetUserByUsernameQuery("user1");
        var query2 = new GetUserByUsernameQuery("USER2");

        // Act
        var tags1 = query1.GetCacheTags();
        var tags2 = query2.GetCacheTags();

        // Assert
        tags1.Should().Contain("user-username:user1");
        tags2.Should().Contain("user-username:user2");
        tags1.Should().Contain("users");
        tags2.Should().Contain("users");
    }

    [Fact]
    public void GetCacheTags_WithMixedCase_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var query1 = new GetUserByUsernameQuery("TestUser");
        var query2 = new GetUserByUsernameQuery("TESTUSER");

        // Act
        var tags1 = query1.GetCacheTags();
        var tags2 = query2.GetCacheTags();

        // Assert
        tags1.Should().Contain("user-username:testuser");
        tags2.Should().Contain("user-username:testuser");
    }

    [Fact]
    public void Query_ShouldImplementICacheableQuery()
    {
        // Arrange
        var query = new GetUserByUsernameQuery("testuser");

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableQuery>();
    }

    [Fact]
    public void Query_ShouldBeQueryOfResultUserDto()
    {
        // Arrange
        var query = new GetUserByUsernameQuery("testuser");

        // Act & Assert
        query.Should().BeAssignableTo<Query<Result<UserDto>>>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_WithInvalidUsername_ShouldStillCreateQuery(string username)
    {
        // Arrange & Act
        var query = new GetUserByUsernameQuery(username);

        // Assert
        query.Should().NotBeNull();
        query.Username.Should().Be(username);
    }

    [Fact]
    public void GetCacheKey_WithEmptyUsername_ShouldHandleGracefully()
    {
        // Arrange
        var query = new GetUserByUsernameQuery("");

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be("user:username:");
    }
}
