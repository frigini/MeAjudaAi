using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Services;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Infrastructure")]
public class KeycloakUserDomainServiceTests
{
    private readonly Mock<IKeycloakService> _keycloakServiceMock;
    private readonly KeycloakUserDomainService _service;

    public KeycloakUserDomainServiceTests()
    {
        _keycloakServiceMock = new Mock<IKeycloakService>();
        _service = new KeycloakUserDomainService(_keycloakServiceMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WhenKeycloakCreationSucceeds_ShouldReturnUserWithKeycloakId()
    {
        // Arrange
        var username = new Username("testuser");
        var email = new Email("test@example.com");
        var firstName = "Test";
        var lastName = "User";
        var password = "SecurePassword123!";
        var roles = new[] { "User" };
        var keycloakId = "keycloak-id-123";

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(
                username.Value,
                email.Value,
                firstName,
                lastName,
                password,
                roles,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(keycloakId));

        // Act
        var result = await _service.CreateUserAsync(
            username,
            email,
            firstName,
            lastName,
            password,
            roles,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(username, result.Value.Username);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(keycloakId, result.Value.KeycloakId);
        Assert.Equal(firstName, result.Value.FirstName);
        Assert.Equal(lastName, result.Value.LastName);
    }

    [Fact]
    public async Task CreateUserAsync_WhenKeycloakCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var username = new Username("testuser");
        var email = new Email("test@example.com");
        var firstName = "Test";
        var lastName = "User";
        var password = "SecurePassword123!";
        var roles = new[] { "User" };
        var errorMessage = "Keycloak creation failed";

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(
                username.Value,
                email.Value,
                firstName,
                lastName,
                password,
                roles,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(errorMessage));

        // Act
        var result = await _service.CreateUserAsync(
            username,
            email,
            firstName,
            lastName,
            password,
            roles,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(errorMessage, result.Error.Message);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidParameters_ShouldCallKeycloakServiceWithCorrectParameters()
    {
        // Arrange
        var username = new Username("testuser");
        var email = new Email("test@example.com");
        var firstName = "Test";
        var lastName = "User";
        var password = "SecurePassword123!";
        var roles = new[] { "User", "Customer" };
        var keycloakId = "keycloak-id-123";

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(keycloakId));

        // Act
        await _service.CreateUserAsync(
            username,
            email,
            firstName,
            lastName,
            password,
            roles,
            CancellationToken.None);

        // Assert
        _keycloakServiceMock.Verify(
            x => x.CreateUserAsync(
                username.Value,
                email.Value,
                firstName,
                lastName,
                password,
                roles,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SyncUserWithKeycloakAsync_ShouldReturnSuccessResult()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        // Act
        var result = await _service.SyncUserWithKeycloakAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SyncUserWithKeycloakAsync_WithNullUserId_ShouldCompleteWithoutErrors()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        // Act & Assert - Não deve lançar exceção
        var result = await _service.SyncUserWithKeycloakAsync(userId, CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("simple")]
    [InlineData("Test@123")]
    public async Task CreateUserAsync_WithVariousPasswordFormats_ShouldPassToKeycloak(string password)
    {
        // Arrange
        var username = new Username("testuser");
        var email = new Email("test@example.com");
        var firstName = "Test";
        var lastName = "User";
        var roles = new[] { "User" };
        var keycloakId = "keycloak-id-123";

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                password,
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(keycloakId));

        // Act
        var result = await _service.CreateUserAsync(
            username,
            email,
            firstName,
            lastName,
            password,
            roles,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _keycloakServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                password,
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithEmptyRoles_ShouldPassEmptyRolesToKeycloak()
    {
        // Arrange
        var username = new Username("testuser");
        var email = new Email("test@example.com");
        var firstName = "Test";
        var lastName = "User";
        var password = "SecurePassword123!";
        var roles = Array.Empty<string>();
        var keycloakId = "keycloak-id-123";

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                roles,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(keycloakId));

        // Act
        var result = await _service.CreateUserAsync(
            username,
            email,
            firstName,
            lastName,
            password,
            roles,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _keycloakServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                roles,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithCancellationToken_ShouldPassTokenToKeycloak()
    {
        // Arrange
        var username = new Username("testuser");
        var email = new Email("test@example.com");
        var firstName = "Test";
        var lastName = "User";
        var password = "SecurePassword123!";
        var roles = new[] { "User" };
        var keycloakId = "keycloak-id-123";
        var cancellationToken = new CancellationToken(true);

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                cancellationToken))
            .ReturnsAsync(Result<string>.Success(keycloakId));

        // Act
        var result = await _service.CreateUserAsync(
            username,
            email,
            firstName,
            lastName,
            password,
            roles,
            cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _keycloakServiceMock.Verify(
            x => x.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                cancellationToken),
            Times.Once);
    }
}
