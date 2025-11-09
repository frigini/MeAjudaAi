using FluentAssertions;
using MeAjudaAi.Modules.Users.Infrastructure.Services.Mock;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Services.Mock;

/// <summary>
/// Tests for MockAuthenticationHelper to verify deterministic and unique behavior
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Component", "MockServices")]
public class MockAuthenticationHelperTests
{
    [Fact]
    public void CreateMockKeycloakId_WithoutParameter_ShouldReturnUniqueIds()
    {
        // Act
        var id1 = MockAuthenticationHelper.CreateMockKeycloakId();
        var id2 = MockAuthenticationHelper.CreateMockKeycloakId();
        var id3 = MockAuthenticationHelper.CreateMockKeycloakId();

        // Assert
        id1.Should().NotBe(id2);
        id2.Should().NotBe(id3);
        id1.Should().NotBe(id3);
        
        // All should start with "keycloak-" prefix
        id1.Should().StartWith("keycloak-");
        id2.Should().StartWith("keycloak-");
        id3.Should().StartWith("keycloak-");
    }

    [Fact]
    public void CreateMockKeycloakId_WithUserSpecificValue_ShouldReturnDeterministicId()
    {
        // Act
        var id1 = MockAuthenticationHelper.CreateMockKeycloakId("user123");
        var id2 = MockAuthenticationHelper.CreateMockKeycloakId("user123");
        var id3 = MockAuthenticationHelper.CreateMockKeycloakId("user456");

        // Assert
        id1.Should().Be(id2); // Same user should get same ID
        id1.Should().NotBe(id3); // Different users should get different IDs
        
        id1.Should().Be("keycloak-user123");
        id3.Should().Be("keycloak-user456");
    }

    [Fact]
    public void CreateMockAuthenticationResult_ShouldReturnValidNonExpiredToken()
    {
        // Act
        var result = MockAuthenticationHelper.CreateMockAuthenticationResult();

        // Assert
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddYears(1000)); // Far future
        result.UserId.Should().NotBe(Guid.Empty);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.Roles.Should().NotBeNull();
    }

    [Fact]
    public void CreateMockTokenValidationResult_ShouldReturnValidResult()
    {
        // Act
        var result = MockAuthenticationHelper.CreateMockTokenValidationResult();

        // Assert
        result.UserId.Should().NotBe(Guid.Empty);
        result.Roles.Should().NotBeNull();
        result.Claims.Should().NotBeNull().And.NotBeEmpty();
        result.Claims.Should().ContainKey("sub");
        result.Claims.Should().ContainKey("preferred_username");
        result.Claims.Should().ContainKey("email");
    }
}