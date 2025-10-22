using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class GetUsersQueryTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateQuery()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var searchTerm = "test";

        // Act
        var query = new GetUsersQuery(page, pageSize, searchTerm);

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(page);
        query.PageSize.Should().Be(pageSize);
        query.SearchTerm.Should().Be(searchTerm);
    }

    [Fact]
    public void Constructor_WithNullSearchTerm_ShouldCreateQuery()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        // Act
        var query = new GetUsersQuery(page, pageSize, null);

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(page);
        query.PageSize.Should().Be(pageSize);
        query.SearchTerm.Should().BeNull();
    }

    [Fact]
    public void GetCacheKey_WithSearchTerm_ShouldReturnCorrectKey()
    {
        // Arrange
        var query = new GetUsersQuery(1, 10, "TestUser");

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be("users:page:1:size:10:search:testuser");
    }

    [Fact]
    public void GetCacheKey_WithNullSearchTerm_ShouldUseAllKeyword()
    {
        // Arrange
        var query = new GetUsersQuery(2, 20, null);

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be("users:page:2:size:20:search:all");
    }

    [Fact]
    public void GetCacheKey_WithEmptySearchTerm_ShouldUseAllKeyword()
    {
        // Arrange
        var query = new GetUsersQuery(1, 15, "");

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be("users:page:1:size:15:search:all");
    }

    [Fact]
    public void GetCacheKey_WithWhitespaceSearchTerm_ShouldUseWhitespaceAsKey()
    {
        // Arrange
        var query = new GetUsersQuery(1, 10, "   ");

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be("users:page:1:size:10:search:   ");
    }

    [Fact]
    public void GetCacheKey_WithDifferentParameters_ShouldReturnDifferentKeys()
    {
        // Arrange
        var query1 = new GetUsersQuery(1, 10, "user1");
        var query2 = new GetUsersQuery(2, 10, "user1");
        var query3 = new GetUsersQuery(1, 20, "user1");
        var query4 = new GetUsersQuery(1, 10, "user2");

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();
        var key3 = query3.GetCacheKey();
        var key4 = query4.GetCacheKey();

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().NotBe(key3);
        key1.Should().NotBe(key4);
        key2.Should().NotBe(key3);
        key2.Should().NotBe(key4);
        key3.Should().NotBe(key4);
    }

    [Fact]
    public void GetCacheKey_WithMixedCaseSearch_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var query1 = new GetUsersQuery(1, 10, "TestUser");
        var query2 = new GetUsersQuery(1, 10, "TESTUSER");
        var query3 = new GetUsersQuery(1, 10, "testuser");

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();
        var key3 = query3.GetCacheKey();

        // Assert
        key1.Should().Be(key2).And.Be(key3);
        key1.Should().Be("users:page:1:size:10:search:testuser");
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn5Minutes()
    {
        // Arrange
        var query = new GetUsersQuery(1, 10, "test");

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var query = new GetUsersQuery(1, 10, "test");

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().HaveCount(2);
        tags.Should().Contain("users");
        tags.Should().Contain("users-list");
    }

    [Fact]
    public void Query_ShouldImplementICacheableQuery()
    {
        // Arrange
        var query = new GetUsersQuery(1, 10, "test");

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableQuery>();
    }

    [Fact]
    public void Query_ShouldBeQueryOfResultPagedUserDto()
    {
        // Arrange
        var query = new GetUsersQuery(1, 10, "test");

        // Act & Assert
        query.Should().BeAssignableTo<Query<Result<PagedResult<UserDto>>>>();
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void Constructor_WithInvalidParameters_ShouldStillCreateQuery(int page, int pageSize)
    {
        // Arrange & Act
        var query = new GetUsersQuery(page, pageSize, "test");

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(page);
        query.PageSize.Should().Be(pageSize);
    }
}
