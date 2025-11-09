using FluentAssertions;
using MeAjudaAi.Modules.Users.Infrastructure.Services.Mock;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Services.Mock;

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
        _service.Reset(); // Clear shared state between tests
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
        result.Value.Should().StartWith("keycloak-");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnSuccessWithValidToken()
    {
        // Act
        var result = await _service.AuthenticateAsync("testuser", "password123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        result.Value.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddYears(1000));
        result.Value.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ValidateTokenAsync_ShouldReturnSuccessWithValidResult()
    {
        // Act
        var result = await _service.ValidateTokenAsync("mock-token");

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

    [Fact]
    public async Task ConcurrentUserCreation_ShouldBeThreadSafe()
    {
        // Arrange
        var userCount = 50;
        var tasks = new List<Task<string>>();

        // Act - Create users concurrently
        for (int i = 0; i < userCount; i++)
        {
            int userId = i; // Capture loop variable
            tasks.Add(Task.Run(async () =>
            {
                var result = await _service.CreateUserAsync($"user{userId}", $"user{userId}@example.com", "Test", "User", "password", new[] { "user" });
                return result.Value;
            }));
        }

        var keycloakIds = await Task.WhenAll(tasks);

        // Assert - All IDs should be unique
        keycloakIds.Should().OnlyHaveUniqueItems();
        keycloakIds.Should().AllSatisfy(id => id.Should().StartWith("keycloak-"));
    }
}
