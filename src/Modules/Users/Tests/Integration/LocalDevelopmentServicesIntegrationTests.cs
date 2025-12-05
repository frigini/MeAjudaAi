using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Services.LocalDevelopment;
using MeAjudaAi.Modules.Users.Tests.Infrastructure;

namespace MeAjudaAi.Modules.Users.Tests.Integration;

/// <summary>
/// Integration tests for LocalDevelopment domain services (used when Keycloak is disabled).
/// </summary>
[Collection("UsersIntegrationTests")]
[Trait("Category", "Integration")]
[Trait("Module", "Users")]
[Trait("Component", "Infrastructure")]
public class LocalDevelopmentServicesIntegrationTests : UsersIntegrationTestBase
{
    private IUserDomainService _userDomainService = null!;
    private IAuthenticationDomainService _authenticationDomainService = null!;

    protected override Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        // Use LocalDevelopment implementations directly
        _userDomainService = new LocalDevelopmentUserDomainService();
        _authenticationDomainService = new LocalDevelopmentAuthenticationDomainService();
        return Task.CompletedTask;
    }

    #region LocalDevelopmentUserDomainService Tests

    [Fact]
    public async Task CreateUserAsync_ShouldCreateUserWithMockKeycloakId()
    {
        // Arrange
        var username = new Username("testuser");
        var email = new Email("test@example.com");
        var firstName = "Test";
        var lastName = "User";
        var password = "password";
        var roles = new[] { "customer" };

        // Act
        var result = await _userDomainService.CreateUserAsync(
            username,
            email,
            firstName,
            lastName,
            password,
            roles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be(username);
        result.Value.Email.Should().Be(email);
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
        result.Value.KeycloakId.Should().StartWith("mock_keycloak_");
    }

    [Fact]
    public async Task CreateUserAsync_WithDifferentCredentials_ShouldGenerateUniqueKeycloakIds()
    {
        // Arrange
        var username1 = new Username("user1");
        var email1 = new Email("user1@example.com");
        var username2 = new Username("user2");
        var email2 = new Email("user2@example.com");

        // Act
        var result1 = await _userDomainService.CreateUserAsync(
            username1, email1, "First", "User", "password", new[] { "customer" });
        var result2 = await _userDomainService.CreateUserAsync(
            username2, email2, "Second", "User", "password", new[] { "customer" });

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value!.KeycloakId.Should().NotBe(result2.Value!.KeycloakId);
    }

    [Fact]
    public async Task CreateUserAsync_WithMultipleRoles_ShouldSucceed()
    {
        // Arrange
        var username = new Username("adminuser");
        var email = new Email("admin@example.com");
        var roles = new[] { "customer", "admin", "provider" };

        // Act
        var result = await _userDomainService.CreateUserAsync(
            username,
            email,
            "Admin",
            "User",
            "password",
            roles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task SyncUserWithKeycloakAsync_ShouldAlwaysReturnSuccess()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        // Act
        var result = await _userDomainService.SyncUserWithKeycloakAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SyncUserWithKeycloakAsync_WithDifferentUserIds_ShouldAlwaysSucceed()
    {
        // Arrange
        var userId1 = new UserId(Guid.NewGuid());
        var userId2 = new UserId(Guid.NewGuid());

        // Act
        var result1 = await _userDomainService.SyncUserWithKeycloakAsync(userId1);
        var result2 = await _userDomainService.SyncUserWithKeycloakAsync(userId2);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region LocalDevelopmentAuthenticationDomainService Tests

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";

        // Act
        var result = await _authenticationDomainService.AuthenticateAsync(username, password);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().StartWith("mock_token_");
        result.Value.RefreshToken.Should().StartWith("mock_refresh_");
        result.Value.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        result.Value.Roles.Should().Contain("customer");
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmailInsteadOfUsername_ShouldReturnSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var password = "testpassword";

        // Act
        var result = await _authenticationDomainService.AuthenticateAsync(email, password);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().StartWith("mock_token_");
    }

    [Theory]
    [InlineData("testuser", "wrongpassword")]
    [InlineData("wronguser", "testpassword")]
    [InlineData("", "testpassword")]
    [InlineData("testuser", "")]
    public async Task AuthenticateAsync_WithInvalidCredentials_ShouldReturnFailure(
        string username,
        string password)
    {
        // Act
        var result = await _authenticationDomainService.AuthenticateAsync(username, password);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task AuthenticateAsync_SameUserMultipleTimes_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";

        // Act
        var result1 = await _authenticationDomainService.AuthenticateAsync(username, password);
        await Task.Delay(1100); // Ensure different timestamps (needs >1s for Unix timestamp to change)
        var result2 = await _authenticationDomainService.AuthenticateAsync(username, password);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value!.AccessToken.Should().NotBe(result2.Value!.AccessToken);
        result1.Value.RefreshToken.Should().NotBe(result2.Value.RefreshToken);
    }

    [Fact]
    public async Task AuthenticateAsync_WithUsername_ShouldGenerateDeterministicUserId()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";

        // Act
        var result1 = await _authenticationDomainService.AuthenticateAsync(username, password);
        var result2 = await _authenticationDomainService.AuthenticateAsync(username, password);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value!.UserId.Should().Be(result2.Value!.UserId); // Same user should have same ID
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidMockToken_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var token = $"mock_token_{userId}_{timestamp}";

        // Act
        var result = await _authenticationDomainService.ValidateTokenAsync(token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(userId);
        result.Value.Roles.Should().Contain("customer");
        result.Value.Claims.Should().ContainKey("sub");
        result.Value.Claims["sub"].Should().Be(userId.ToString());
    }

    [Fact]
    public async Task ValidateTokenAsync_WithMalformedMockToken_ShouldUseFallbackUserId()
    {
        // Arrange
        var token = "mock_token_invalid";

        // Act
        var result = await _authenticationDomainService.ValidateTokenAsync(token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().NotBeEmpty();
        result.Value.Roles.Should().Contain("customer");
    }

    [Theory]
    [InlineData("invalid_token")]
    [InlineData("bearer_token_xyz")]
    [InlineData("")]
    [InlineData("token_mock_123")] // Wrong prefix
    public async Task ValidateTokenAsync_WithInvalidToken_ShouldReturnFailure(string token)
    {
        // Act
        var result = await _authenticationDomainService.ValidateTokenAsync(token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Invalid token");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithTokenFromAuthenticate_ShouldExtractCorrectUserId()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";
        var authResult = await _authenticationDomainService.AuthenticateAsync(username, password);
        authResult.IsSuccess.Should().BeTrue();

        // Act
        var validateResult = await _authenticationDomainService.ValidateTokenAsync(
            authResult.Value!.AccessToken);

        // Assert
        validateResult.IsSuccess.Should().BeTrue();
        validateResult.Value!.UserId.Should().Be(authResult.Value.UserId);
    }

    [Fact]
    public async Task AuthenticateAndValidate_FullWorkflow_ShouldMaintainUserIdConsistency()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";

        // Act - Authenticate
        var authResult = await _authenticationDomainService.AuthenticateAsync(username, password);

        // Act - Validate the token
        var validateResult = await _authenticationDomainService.ValidateTokenAsync(
            authResult.Value!.AccessToken);

        // Assert
        authResult.IsSuccess.Should().BeTrue();
        validateResult.IsSuccess.Should().BeTrue();
        validateResult.Value!.UserId.Should().Be(authResult.Value.UserId);
        validateResult.Value.Roles.Should().Equal(authResult.Value.Roles);
    }

    #endregion
}
