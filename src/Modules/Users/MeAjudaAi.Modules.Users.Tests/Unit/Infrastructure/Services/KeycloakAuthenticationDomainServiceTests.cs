using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Services;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Infrastructure")]
public class KeycloakAuthenticationDomainServiceTests
{
    private readonly Mock<IKeycloakService> _keycloakServiceMock;
    private readonly KeycloakAuthenticationDomainService _service;

    public KeycloakAuthenticationDomainServiceTests()
    {
        _keycloakServiceMock = new Mock<IKeycloakService>();
        _service = new KeycloakAuthenticationDomainService(_keycloakServiceMock.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenKeycloakAuthenticationSucceeds_ShouldReturnSuccessResult()
    {
        // Arrange
        var usernameOrEmail = "test@example.com";
        var password = "SecurePassword123!";
        var expectedResult = Result<AuthenticationResult>.Success(
            new AuthenticationResult(
                UserId: Guid.NewGuid(),
                AccessToken: "access-token",
                RefreshToken: "refresh-token",
                ExpiresAt: DateTime.UtcNow.AddHours(1),
                Roles: ["User", "Customer"]
            ));

        _keycloakServiceMock
            .Setup(x => x.AuthenticateAsync(usernameOrEmail, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.AuthenticateAsync(usernameOrEmail, password, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedResult.Value.AccessToken, result.Value.AccessToken);
        Assert.Equal(expectedResult.Value.RefreshToken, result.Value.RefreshToken);
        Assert.Equal(expectedResult.Value.ExpiresAt, result.Value.ExpiresAt);
        Assert.Equal(expectedResult.Value.UserId, result.Value.UserId);
        Assert.Equal(expectedResult.Value.Roles, result.Value.Roles);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenKeycloakAuthenticationFails_ShouldReturnFailureResult()
    {
        // Arrange
        var usernameOrEmail = "test@example.com";
        var password = "WrongPassword";
        var errorMessage = "Invalid credentials";
        var expectedResult = Result<AuthenticationResult>.Failure(errorMessage);

        _keycloakServiceMock
            .Setup(x => x.AuthenticateAsync(usernameOrEmail, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.AuthenticateAsync(usernameOrEmail, password, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(errorMessage, result.Error.Message);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldCallKeycloakServiceWithCorrectParameters()
    {
        // Arrange
        var usernameOrEmail = "testuser";
        var password = "SecurePassword123!";
        var cancellationToken = new CancellationToken();
        var expectedResult = Result<AuthenticationResult>.Success(
            new AuthenticationResult());

        _keycloakServiceMock
            .Setup(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _service.AuthenticateAsync(usernameOrEmail, password, cancellationToken);

        // Assert
        _keycloakServiceMock.Verify(
            x => x.AuthenticateAsync(usernameOrEmail, password, cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("testuser")]
    [InlineData("another.user@domain.org")]
    public async Task AuthenticateAsync_WithDifferentUsernameFormats_ShouldPassToKeycloak(string usernameOrEmail)
    {
        // Arrange
        var password = "SecurePassword123!";
        var expectedResult = Result<AuthenticationResult>.Success(
            new AuthenticationResult());

        _keycloakServiceMock
            .Setup(x => x.AuthenticateAsync(usernameOrEmail, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.AuthenticateAsync(usernameOrEmail, password, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _keycloakServiceMock.Verify(
            x => x.AuthenticateAsync(usernameOrEmail, password, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenKeycloakValidationSucceeds_ShouldReturnSuccessResult()
    {
        // Arrange
        var token = "valid-jwt-token";
        var expectedResult = Result<TokenValidationResult>.Success(
            new TokenValidationResult(
                UserId: Guid.NewGuid(),
                Roles: ["User", "Customer"],
                Claims: new Dictionary<string, object> { ["sub"] = "user-123" }
            ));

        _keycloakServiceMock
            .Setup(x => x.ValidateTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ValidateTokenAsync(token, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedResult.Value.UserId, result.Value.UserId);
        Assert.Equal(expectedResult.Value.Roles, result.Value.Roles);
        Assert.Equal(expectedResult.Value.Claims, result.Value.Claims);
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenKeycloakValidationFails_ShouldReturnFailureResult()
    {
        // Arrange
        var token = "invalid-jwt-token";
        var errorMessage = "Invalid token";
        var expectedResult = Result<TokenValidationResult>.Failure(errorMessage);

        _keycloakServiceMock
            .Setup(x => x.ValidateTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ValidateTokenAsync(token, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(errorMessage, result.Error.Message);
    }

    [Fact]
    public async Task ValidateTokenAsync_ShouldCallKeycloakServiceWithCorrectParameters()
    {
        // Arrange
        var token = "test-jwt-token";
        var cancellationToken = new CancellationToken();
        var expectedResult = Result<TokenValidationResult>.Success(
            new TokenValidationResult());

        _keycloakServiceMock
            .Setup(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _service.ValidateTokenAsync(token, cancellationToken);

        // Assert
        _keycloakServiceMock.Verify(
            x => x.ValidateTokenAsync(token, cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid.token")]
    [InlineData("Bearer valid-token")]
    public async Task ValidateTokenAsync_WithDifferentTokenFormats_ShouldPassToKeycloak(string token)
    {
        // Arrange
        var expectedResult = Result<TokenValidationResult>.Success(
            new TokenValidationResult());

        _keycloakServiceMock
            .Setup(x => x.ValidateTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ValidateTokenAsync(token, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _keycloakServiceMock.Verify(
            x => x.ValidateTokenAsync(token, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithCancellationToken_ShouldPassTokenToKeycloak()
    {
        // Arrange
        var token = "test-jwt-token";
        var cancellationToken = new CancellationToken(true);
        var expectedResult = Result<TokenValidationResult>.Success(
            new TokenValidationResult());

        _keycloakServiceMock
            .Setup(x => x.ValidateTokenAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ValidateTokenAsync(token, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _keycloakServiceMock.Verify(
            x => x.ValidateTokenAsync(token, cancellationToken),
            Times.Once);
    }
}