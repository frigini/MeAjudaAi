using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class GetUserByEmailQueryTests
{
    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateQuery()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var query = new GetUserByEmailQuery(email);

        // Assert
        query.Should().NotBeNull();
        query.Email.Should().Be(email);
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var email = "Test@Example.Com";
        var query = new GetUserByEmailQuery(email);

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be("user:email:test@example.com");
    }

    [Fact]
    public void GetCacheKey_WithDifferentEmails_ShouldReturnDifferentKeys()
    {
        // Arrange
        var query1 = new GetUserByEmailQuery("user1@example.com");
        var query2 = new GetUserByEmailQuery("user2@example.com");

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().Be("user:email:user1@example.com");
        key2.Should().Be("user:email:user2@example.com");
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn15Minutes()
    {
        // Arrange
        var query = new GetUserByEmailQuery("test@example.com");

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var email = "Test@Example.Com";
        var query = new GetUserByEmailQuery(email);

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().HaveCount(2);
        tags.Should().Contain("users");
        tags.Should().Contain("user-email:test@example.com");
    }

    [Fact]
    public void GetCacheTags_WithDifferentEmails_ShouldReturnDifferentUserTags()
    {
        // Arrange
        var query1 = new GetUserByEmailQuery("user1@example.com");
        var query2 = new GetUserByEmailQuery("USER2@EXAMPLE.COM");

        // Act
        var tags1 = query1.GetCacheTags();
        var tags2 = query2.GetCacheTags();

        // Assert
        tags1.Should().Contain("user-email:user1@example.com");
        tags2.Should().Contain("user-email:user2@example.com");
        tags1.Should().Contain("users");
        tags2.Should().Contain("users");
    }

    [Fact]
    public void Query_ShouldImplementICacheableQuery()
    {
        // Arrange
        var query = new GetUserByEmailQuery("test@example.com");

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableQuery>();
    }

    [Fact]
    public void Query_ShouldBeQueryOfResultUserDto()
    {
        // Arrange
        var query = new GetUserByEmailQuery("test@example.com");

        // Act & Assert
        query.Should().BeAssignableTo<Query<Result<UserDto>>>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_WithInvalidEmail_ShouldStillCreateQuery(string email)
    {
        // Arrange & Act
        var query = new GetUserByEmailQuery(email);

        // Assert
        query.Should().NotBeNull();
        query.Email.Should().Be(email);
    }

    [Fact]
    public void GetCacheKey_WithEmptyEmail_ShouldHandleGracefully()
    {
        // Arrange
        var query = new GetUserByEmailQuery("");

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be("user:email:");
    }
}