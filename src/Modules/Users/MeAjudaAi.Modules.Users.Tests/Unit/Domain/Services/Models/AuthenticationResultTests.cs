using MeAjudaAi.Modules.Users.Domain.Services.Models;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.Services.Models;

[Trait("Category", "Unit")]
public class AuthenticationResultTests
{
    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateInstance()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string accessToken = "access_token_value";
        const string refreshToken = "refresh_token_value";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var roles = new[] { "Admin", "User" };

        // Act
        var result = new AuthenticationResult(userId, accessToken, refreshToken, expiresAt, roles);

        // Assert
        result.UserId.Should().Be(userId);
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.ExpiresAt.Should().Be(expiresAt);
        result.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void Constructor_WithDefaultValues_ShouldCreateInstanceWithNulls()
    {
        // Act
        var result = new AuthenticationResult();

        // Assert
        result.UserId.Should().BeNull();
        result.AccessToken.Should().BeNull();
        result.RefreshToken.Should().BeNull();
        result.ExpiresAt.Should().BeNull();
        result.Roles.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithPartialParameters_ShouldCreateInstanceWithProvidedValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string accessToken = "partial_access_token";

        // Act
        var result = new AuthenticationResult(UserId: userId, AccessToken: accessToken);

        // Assert
        result.UserId.Should().Be(userId);
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().BeNull();
        result.ExpiresAt.Should().BeNull();
        result.Roles.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyRoles_ShouldAcceptEmptyEnumerable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emptyRoles = Array.Empty<string>();

        // Act
        var result = new AuthenticationResult(UserId: userId, Roles: emptyRoles);

        // Assert
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMultipleRoles_ShouldPreserveAllRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roles = new[] { "SuperAdmin", "Admin", "User", "Guest" };

        // Act
        var result = new AuthenticationResult(UserId: userId, Roles: roles);

        // Assert
        result.Roles.Should().HaveCount(4);
        result.Roles.Should().ContainInOrder("SuperAdmin", "Admin", "User", "Guest");
    }

    [Fact]
    public void Constructor_WithPastExpirationDate_ShouldAllowPastDates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pastDate = DateTime.UtcNow.AddHours(-1);

        // Act
        var result = new AuthenticationResult(UserId: userId, ExpiresAt: pastDate);

        // Assert
        result.ExpiresAt.Should().Be(pastDate);
        result.ExpiresAt.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string accessToken = "token";
        const string refreshToken = "refresh";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var roles = new[] { "Admin" };

        var result1 = new AuthenticationResult(userId, accessToken, refreshToken, expiresAt, roles);
        var result2 = new AuthenticationResult(userId, accessToken, refreshToken, expiresAt, roles);

        // Act & Assert
        result1.Should().Be(result2);
        result1.GetHashCode().Should().Be(result2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var result1 = new AuthenticationResult(UserId: userId1);
        var result2 = new AuthenticationResult(UserId: userId2);

        // Act & Assert
        result1.Should().NotBe(result2);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("valid_token")]
    public void Constructor_WithVariousTokenValues_ShouldAcceptAllStringValues(string token)
    {
        // Act
        var result = new AuthenticationResult(AccessToken: token, RefreshToken: token);

        // Assert
        result.AccessToken.Should().Be(token);
        result.RefreshToken.Should().Be(token);
    }
}