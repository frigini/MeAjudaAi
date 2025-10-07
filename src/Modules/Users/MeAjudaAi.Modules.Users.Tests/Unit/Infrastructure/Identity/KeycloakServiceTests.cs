using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak.Models;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Identity;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Component", "KeycloakService")]
public class KeycloakServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _options;
    private readonly Mock<ILogger<KeycloakService>> _mockLogger;
    private readonly KeycloakService _keycloakService;

    public KeycloakServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _options = new KeycloakOptions
        {
            BaseUrl = "https://keycloak.example.com",
            Realm = "test-realm",
            ClientId = "test-client",
            ClientSecret = "test-secret",
            AdminUsername = "admin",
            AdminPassword = "admin-password"
        };

        _mockLogger = new Mock<ILogger<KeycloakService>>();
        _keycloakService = new KeycloakService(_httpClient, _options, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WhenAdminTokenFails_ShouldReturnFailure()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Unauthorized, "");

        // Act
        var result = await _keycloakService.CreateUserAsync(
            "testuser",
            "test@example.com",
            "Test",
            "User",
            "password",
            ["user"]);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Failed to authenticate admin user");
    }

    [Fact]
    public async Task CreateUserAsync_WhenValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var adminTokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "admin-token",
            ExpiresIn = 3600,
            RefreshToken = "refresh-token",
            TokenType = "Bearer"
        };

        var userId = Guid.NewGuid().ToString();

        // Configura resposta do token de admin
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(adminTokenResponse));

        // Configura resposta de cria��o de usu�rio com cabe�alho Location
        var userCreationResponse = new HttpResponseMessage(HttpStatusCode.Created);
        userCreationResponse.Headers.Location = new Uri($"https://keycloak.example.com/admin/realms/test-realm/users/{userId}");

        _mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse))
            })
            .ReturnsAsync(userCreationResponse);

        // Act
        var result = await _keycloakService.CreateUserAsync(
            "testuser",
            "test@example.com",
            "Test",
            "User",
            "password",
            []);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(userId);
    }

    [Fact]
    public async Task CreateUserAsync_WhenUserCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var adminTokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "admin-token",
            ExpiresIn = 3600,
            RefreshToken = "refresh-token",
            TokenType = "Bearer"
        };

        // Configura sequ�ncia de respostas simulando falha na cria��o do usu�rio
        _mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse))
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("User already exists")
            });

        // Act
        var result = await _keycloakService.CreateUserAsync(
            "testuser",
            "test@example.com",
            "Test",
            "User",
            "password",
            []);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Failed to create user in Keycloak: BadRequest");
    }

    [Fact]
    public async Task CreateUserAsync_WhenLocationHeaderMissing_ShouldReturnFailure()
    {
        // Arrange
        var adminTokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "admin-token",
            ExpiresIn = 3600,
            RefreshToken = "refresh-token",
            TokenType = "Bearer"
        };

        // Configura resposta sem cabe�alho Location
        _mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse))
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        // Act
        var result = await _keycloakService.CreateUserAsync(
            "testuser",
            "test@example.com",
            "Test",
            "User",
            "password",
            []);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Failed to get user ID from Keycloak response");
    }

    [Fact]
    public async Task CreateUserAsync_WhenExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        // Simula exce��o de rede
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _keycloakService.CreateUserAsync(
            "testuser",
            "test@example.com",
            "Test",
            "User",
            "password",
            []);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Admin token request failed: Network error");
    }

    [Fact]
    public async Task AuthenticateAsync_WhenValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var jwtToken = CreateValidJwtToken();
        var tokenResponse = new KeycloakTokenResponse
        {
            AccessToken = jwtToken,
            ExpiresIn = 3600,
            RefreshToken = "refresh-token",
            TokenType = "Bearer"
        };

        // Configura resposta simulando autenticação bem-sucedida
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));

        // Act
        var result = await _keycloakService.AuthenticateAsync("testuser", "password");

        // Assert with detailed error message for CI debugging
        if (result.IsFailure)
        {
            var errorDetails = $"Authentication failed. Error: {result.Error?.Message ?? "Unknown"}, " +
                             $"JWT Token Length: {jwtToken.Length}, " +
                             $"Token starts with: {(jwtToken.Length > 50 ? jwtToken.Substring(0, 50) : jwtToken)}...";
            Assert.Fail(errorDetails);
        }

        result.IsSuccess.Should().BeTrue("Authentication should succeed with valid credentials");
        result.Value!.AccessToken.Should().Be(tokenResponse.AccessToken);
        result.Value.UserId.Should().NotBe(Guid.Empty, "UserId should be extracted from JWT token");
    }

    [Fact]
    public async Task AuthenticateAsync_DiagnosticTest_ShouldShowDetails()
    {
        // Arrange
        var jwtToken = CreateValidJwtToken();
        var tokenResponse = new KeycloakTokenResponse
        {
            AccessToken = jwtToken,
            ExpiresIn = 3600,
            RefreshToken = "refresh-token",
            TokenType = "Bearer"
        };

        // Decode JWT payload to check structure for debugging
        var parts = jwtToken.Split('.');
        string payloadInfo = "Invalid JWT structure";
        if (parts.Length > 1)
        {
            try
            {
                var payload = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1] + "=="));
                payloadInfo = payload;
            }
            catch
            {
                payloadInfo = "Failed to decode JWT payload";
            }
        }

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));

        // Act
        var result = await _keycloakService.AuthenticateAsync("testuser", "password");

        // Assert with detailed debugging information
        if (result.IsFailure)
        {
            var debugInfo = $"Diagnostic Test Failed. " +
                          $"JWT Payload: {payloadInfo}, " +
                          $"Error: {result.Error?.Message ?? "None"}";
            Assert.Fail(debugInfo);
        }

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        // Configura resposta simulando credenciais inv�lidas
        SetupHttpResponse(HttpStatusCode.Unauthorized, "Invalid credentials");

        // Act
        var result = await _keycloakService.AuthenticateAsync("testuser", "wrongpassword");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Invalid username/email or password");
    }

    [Fact]
    public async Task AuthenticateAsync_WhenNullTokenResponse_ShouldReturnFailure()
    {
        // Arrange
        // Configura resposta simulando retorno nulo do token
        SetupHttpResponse(HttpStatusCode.OK, "null");

        // Act
        var result = await _keycloakService.AuthenticateAsync("testuser", "password");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Invalid token response from Keycloak");
    }

    [Fact]
    public async Task AuthenticateAsync_WhenInvalidJwtToken_ShouldReturnFailure()
    {
        // Arrange
        var tokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "invalid.jwt.token",
            ExpiresIn = 3600,
            RefreshToken = "refresh-token",
            TokenType = "Bearer"
        };

        // Configura resposta simulando token JWT inv�lido
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));

        // Act
        var result = await _keycloakService.AuthenticateAsync("testuser", "password");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().NotBeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        // Simula exce��o de timeout
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _keycloakService.AuthenticateAsync("testuser", "password");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Authentication failed: Request timeout");
    }

    [Fact]
    public async Task DeactivateUserAsync_WhenValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var adminTokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "admin-token",
            ExpiresIn = 3600,
            RefreshToken = "refresh-token",
            TokenType = "Bearer"
        };

        // Configura sequ�ncia de respostas simulando desativa��o bem-sucedida
        _mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse))
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

        // Act
        var result = await _keycloakService.DeactivateUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateUserAsync_WhenAdminTokenFails_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupHttpResponse(HttpStatusCode.Unauthorized, "");

        // Act
        var result = await _keycloakService.DeactivateUserAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Failed to authenticate admin user");
    }

    [Fact]
    public async Task DeactivateUserAsync_WhenDeactivationFails_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var adminTokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "admin-token",
            ExpiresIn = 3600,
            RefreshToken = "refresh-token",
            TokenType = "Bearer"
        };

        // Configura sequ�ncia de respostas simulando falha na desativa��o
        _mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse))
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("User not found")
            });

        // Act
        var result = await _keycloakService.DeactivateUserAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Failed to deactivate user: NotFound");
    }

    [Fact]
    public async Task DeactivateUserAsync_WhenExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        // Simula exce��o de servi�o
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        // Act
        var result = await _keycloakService.DeactivateUserAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Admin token request failed: Service unavailable");
    }

    // Configura resposta simulada para requisi��es HTTP
    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    // Cria um token JWT v�lido para testes
    private static string CreateValidJwtToken()
    {
        var userId = Guid.NewGuid();
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($$"""
        {
            "sub": "{{userId}}",
            "exp": {{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}},
            "iat": {{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}},
            "realm_access": {"roles":["user"]}
        }
        """));
        var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes("signature"));

        return $"{header}.{payload}.{signature}";
    }
}