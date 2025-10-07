using MeAjudaAi.Modules.Users.Domain.Services.Models;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.Services.Models;

[Trait("Category", "Unit")]
public class TokenValidationResultTests
{
    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateInstance()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roles = new[] { "Admin", "User" };
        var claims = new Dictionary<string, object>
        {
            { "name", "John Doe" },
            { "email", "john@example.com" },
            { "age", 30 }
        };

        // Act
        var result = new TokenValidationResult(userId, roles, claims);

        // Assert
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
        result.Claims.Should().BeEquivalentTo(claims);
    }

    [Fact]
    public void Constructor_WithDefaultValues_ShouldCreateInstanceWithNulls()
    {
        // Act
        var result = new TokenValidationResult();

        // Assert
        result.UserId.Should().BeNull();
        result.Roles.Should().BeNull();
        result.Claims.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithPartialParameters_ShouldCreateInstanceWithProvidedValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roles = new[] { "User" };

        // Act
        var result = new TokenValidationResult(UserId: userId, Roles: roles);

        // Assert
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
        result.Claims.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyRoles_ShouldAcceptEmptyEnumerable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emptyRoles = Array.Empty<string>();

        // Act
        var result = new TokenValidationResult(UserId: userId, Roles: emptyRoles);

        // Assert
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyClaims_ShouldAcceptEmptyDictionary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emptyClaims = new Dictionary<string, object>();

        // Act
        var result = new TokenValidationResult(UserId: userId, Claims: emptyClaims);

        // Assert
        result.UserId.Should().Be(userId);
        result.Claims.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMultipleRoles_ShouldPreserveAllRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roles = new[] { "SuperAdmin", "Admin", "Moderator", "User" };

        // Act
        var result = new TokenValidationResult(UserId: userId, Roles: roles);

        // Assert
        result.Roles.Should().HaveCount(4);
        result.Roles.Should().ContainInOrder("SuperAdmin", "Admin", "Moderator", "User");
    }

    [Fact]
    public void Constructor_WithVariousClaimTypes_ShouldAcceptDifferentObjectTypes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new Dictionary<string, object>
        {
            { "string_claim", "text_value" },
            { "number_claim", 42 },
            { "boolean_claim", true },
            { "date_claim", DateTime.UtcNow },
            { "array_claim", new[] { "item1", "item2" } }
        };

        // Act
        var result = new TokenValidationResult(UserId: userId, Claims: claims);

        // Assert
        result.Claims.Should().HaveCount(5);
        result.Claims!["string_claim"].Should().Be("text_value");
        result.Claims["number_claim"].Should().Be(42);
        result.Claims["boolean_claim"].Should().Be(true);
        result.Claims["date_claim"].Should().BeOfType<DateTime>();
        result.Claims["array_claim"].Should().BeOfType<string[]>();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roles = new[] { "Admin" };
        var claims = new Dictionary<string, object> { { "test", "value" } };

        var result1 = new TokenValidationResult(userId, roles, claims);
        var result2 = new TokenValidationResult(userId, roles, claims);

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

        var result1 = new TokenValidationResult(UserId: userId1);
        var result2 = new TokenValidationResult(UserId: userId2);

        // Act & Assert
        result1.Should().NotBe(result2);
    }

    [Fact]
    public void Constructor_WithNullClaimValues_ShouldAcceptNullValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new Dictionary<string, object>
        {
            { "null_claim", null! },
            { "valid_claim", "value" }
        };

        // Act
        var result = new TokenValidationResult(UserId: userId, Claims: claims);

        // Assert
        result.Claims.Should().HaveCount(2);
        result.Claims!["null_claim"].Should().BeNull();
        result.Claims["valid_claim"].Should().Be("value");
    }

    [Theory]
    [InlineData("single_role")]
    [InlineData("")]
    public void Constructor_WithSingleRole_ShouldHandleVariousRoleValues(string role)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roles = new[] { role };

        // Act
        var result = new TokenValidationResult(UserId: userId, Roles: roles);

        // Assert
        result.Roles.Should().HaveCount(1);
        result.Roles!.First().Should().Be(role);
    }
}