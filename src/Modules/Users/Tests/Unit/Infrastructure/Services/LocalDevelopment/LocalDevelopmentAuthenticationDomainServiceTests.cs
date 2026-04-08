using FluentAssertions;
using MeAjudaAi.Modules.Users.Infrastructure.Services.LocalDevelopment;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Services.LocalDevelopment;

[Trait("Category", "Unit")]
public class LocalDevelopmentAuthenticationDomainServiceTests
{
    private readonly LocalDevelopmentAuthenticationDomainService _service = new();

    [Theory]
    [InlineData("testuser", "testpassword")]
    [InlineData("test@example.com", "testpassword")]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnSuccess(string username, string password)
    {
        // Act
        var result = await _service.AuthenticateAsync(username, password);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Roles.Should().Contain("customer");
        result.Value.AccessToken.Should().StartWith($"mock_token_{result.Value.UserId}");
    }

    [Theory]
    [InlineData("testuser", "wrong")]
    [InlineData("wrong", "testpassword")]
    [InlineData("invalid", "invalid")]
    public async Task AuthenticateAsync_WithInvalidCredentials_ShouldReturnFailure(string username, string password)
    {
        // Act
        var result = await _service.AuthenticateAsync(username, password);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidTokenFormat_ShouldExtractUserId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var token = $"mock_token_{expectedId}_timestamp";

        // Act
        var result = await _service.ValidateTokenAsync(token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(expectedId);
        result.Value.Roles.Should().Contain("customer");
        result.Value.Claims["sub"].Should().Be(expectedId.ToString());
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidPrefixButNoGuid_ShouldReturnFallbackUserId()
    {
        // Arrange
        var token = "mock_token_invalid_format";

        // Act
        var result = await _service.ValidateTokenAsync(token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidPrefix_ShouldReturnFailure()
    {
        // Arrange
        var token = "invalid_token_format";

        // Act
        var result = await _service.ValidateTokenAsync(token);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
