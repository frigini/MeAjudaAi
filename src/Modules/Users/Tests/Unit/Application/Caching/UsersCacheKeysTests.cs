using MeAjudaAi.Modules.Users.Application.Caching;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Caching;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class UsersCacheKeysTests
{
    [Fact]
    public void UserById_WithValidGuid_ShouldReturnCorrectKey()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var key = UsersCacheKeys.UserById(userId);

        // Assert
        key.Should().Be($"user:id:{userId}");
    }

    [Fact]
    public void UserById_WithEmptyGuid_ShouldReturnCorrectKey()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var key = UsersCacheKeys.UserById(userId);

        // Assert
        key.Should().Be($"user:id:{Guid.Empty}");
    }

    [Fact]
    public void UserById_WithDifferentGuids_ShouldReturnDifferentKeys()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        // Act
        var key1 = UsersCacheKeys.UserById(userId1);
        var key2 = UsersCacheKeys.UserById(userId2);

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void UserByEmail_WithValidEmail_ShouldReturnCorrectKey()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var key = UsersCacheKeys.UserByEmail(email);

        // Assert
        key.Should().Be("user:email:test@example.com");
    }

    [Fact]
    public void UserByEmail_WithMixedCaseEmail_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var email = "Test@Example.COM";

        // Act
        var key = UsersCacheKeys.UserByEmail(email);

        // Assert
        key.Should().Be("user:email:test@example.com");
    }

    [Fact]
    public void UserByEmail_WithDifferentEmails_ShouldReturnDifferentKeys()
    {
        // Arrange
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";

        // Act
        var key1 = UsersCacheKeys.UserByEmail(email1);
        var key2 = UsersCacheKeys.UserByEmail(email2);

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().Be("user:email:user1@example.com");
        key2.Should().Be("user:email:user2@example.com");
    }

    [Fact]
    public void UsersList_WithoutFilter_ShouldReturnCorrectKey()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        // Act
        var key = UsersCacheKeys.UsersList(page, pageSize);

        // Assert
        key.Should().Be("users:list:1:10");
    }

    [Fact]
    public void UsersList_WithFilter_ShouldReturnCorrectKey()
    {
        // Arrange
        var page = 2;
        var pageSize = 20;
        var filter = "active";

        // Act
        var key = UsersCacheKeys.UsersList(page, pageSize, filter);

        // Assert
        key.Should().Be("users:list:2:20:filter:active");
    }

    [Fact]
    public void UsersList_WithEmptyFilter_ShouldIgnoreFilter()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var filter = "";

        // Act
        var key = UsersCacheKeys.UsersList(page, pageSize, filter);

        // Assert
        key.Should().Be("users:list:1:10");
    }

    [Fact]
    public void UsersList_WithNullFilter_ShouldIgnoreFilter()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        // Act
        var key = UsersCacheKeys.UsersList(page, pageSize, null);

        // Assert
        key.Should().Be("users:list:1:10");
    }

    [Fact]
    public void UsersCount_WithoutFilter_ShouldReturnCorrectKey()
    {
        // Act
        var key = UsersCacheKeys.UsersCount();

        // Assert
        key.Should().Be("users:count");
    }

    [Fact]
    public void UsersCount_WithFilter_ShouldReturnCorrectKey()
    {
        // Arrange
        var filter = "active";

        // Act
        var key = UsersCacheKeys.UsersCount(filter);

        // Assert
        key.Should().Be("users:count:filter:active");
    }

    [Fact]
    public void UsersCount_WithEmptyFilter_ShouldIgnoreFilter()
    {
        // Arrange
        var filter = "";

        // Act
        var key = UsersCacheKeys.UsersCount(filter);

        // Assert
        key.Should().Be("users:count");
    }

    [Fact]
    public void UsersCount_WithNullFilter_ShouldIgnoreFilter()
    {
        // Act
        var key = UsersCacheKeys.UsersCount(null);

        // Assert
        key.Should().Be("users:count");
    }

    [Fact]
    public void UserRoles_WithValidGuid_ShouldReturnCorrectKey()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var key = UsersCacheKeys.UserRoles(userId);

        // Assert
        key.Should().Be($"user:roles:{userId}");
    }

    [Fact]
    public void UserRoles_WithDifferentGuids_ShouldReturnDifferentKeys()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        // Act
        var key1 = UsersCacheKeys.UserRoles(userId1);
        var key2 = UsersCacheKeys.UserRoles(userId2);

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void UserSystemConfig_ShouldReturnConstantValue()
    {
        // Act
        var key = UsersCacheKeys.UserSystemConfig;

        // Assert
        key.Should().Be("user-system-config");
    }

    [Fact]
    public void UserStats_ShouldReturnConstantValue()
    {
        // Act
        var key = UsersCacheKeys.UserStats;

        // Assert
        key.Should().Be("user-stats");
    }

    [Fact]
    public void UserSystemConfig_ShouldBeSameInstanceEachTime()
    {
        // Act
        var key1 = UsersCacheKeys.UserSystemConfig;
        var key2 = UsersCacheKeys.UserSystemConfig;

        // Assert
        key1.Should().BeSameAs(key2);
    }

    [Fact]
    public void UserStats_ShouldBeSameInstanceEachTime()
    {
        // Act
        var key1 = UsersCacheKeys.UserStats;
        var key2 = UsersCacheKeys.UserStats;

        // Assert
        key1.Should().BeSameAs(key2);
    }
}