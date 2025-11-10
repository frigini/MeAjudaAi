using FluentAssertions;
using MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Mocks.Services;

/// <summary>
/// Tests for MockKeycloakService to verify core functionality
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Component", "MockServices")]
public class MockKeycloakServiceTests
{
    private readonly MockKeycloakService _service;

    public MockKeycloakServiceTests()
    {
        _service = new MockKeycloakService();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnSuccessWithKeycloakId()
    {
        // Arrange
        var username = "testuser";
        var email = "test@example.com";
        var firstName = "Test";
        var lastName = "User";
        var password = "password123";
        var roles = new[] { "user", "admin" };

        // Act
        var result = await _service.CreateUserAsync(username, email, firstName, lastName, password, roles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Should().StartWith("keycloak_");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnSuccessWithValidToken()
    {
        // Act
        var result = await _service.AuthenticateAsync("validuser", "validpassword");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        result.Value.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        result.Value.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ValidateTokenAsync_ShouldReturnSuccessWithValidResult()
    {
        // Act
        var result = await _service.ValidateTokenAsync("mock_token_123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().NotBe(Guid.Empty);
        result.Value.Claims.Should().NotBeNull().And.NotBeEmpty();
        result.Value.Roles.Should().NotBeNull();
    }

    [Fact]
    public async Task DeactivateUserAsync_ShouldReturnSuccess()
    {
        // Arrange
        var createResult = await _service.CreateUserAsync("testuser", "test@example.com", "Test", "User", "password", new[] { "user" });
        var keycloakId = createResult.Value;

        // Act
        var result = await _service.DeactivateUserAsync(keycloakId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }


}
