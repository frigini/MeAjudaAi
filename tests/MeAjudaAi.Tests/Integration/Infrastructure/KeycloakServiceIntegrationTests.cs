using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak.Models;
using MeAjudaAi.Shared.Tests.Infrastructure;
using MeAjudaAi.Tests.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MeAjudaAi.Tests.Integration.Infrastructure;

[Collection("Database")]
public class KeycloakServiceIntegrationTests : BaseIntegrationTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly KeycloakService _service;
    private readonly KeycloakOptions _options;

    public KeycloakServiceIntegrationTests(TestApplicationFactory factory) : base(factory)
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        
        _options = new KeycloakOptions
        {
            ServerUrl = TestUrls.LocalhostKeycloak,
            Realm = "test-realm",
            ClientId = "test-client",
            ClientSecret = "test-secret",
            AdminUsername = "admin",
            AdminPassword = "admin"
        };

        var optionsMock = new Mock<IOptions<KeycloakOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);
        
        var logger = Services.GetRequiredService<ILogger<KeycloakService>>();
        _service = new KeycloakService(_httpClient, optionsMock.Object, logger);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnSuccessResult()
    {
        // Arrange
        var username = "testuser";
        var password = "testpass";
        
        var tokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "valid-access-token",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            RefreshToken = "refresh-token"
        };

        var responseContent = JsonSerializer.Serialize(tokenResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains("token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.AuthenticateAsync(username, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.AccessToken.Should().Be("valid-access-token");
        result.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidCredentials_ShouldReturnFailureResult()
    {
        // Arrange
        var username = "invaliduser";
        var password = "wrongpass";
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{\"error\":\"invalid_grant\"}", Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains("token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.AuthenticateAsync(username, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Authentication failed");
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var email = Email.Create("newuser@example.com");
        var username = Username.Create("newuser");
        var password = "SecurePassword123!";
        
        // Simula requisição de token de admin
        var adminTokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "admin-token",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        var adminTokenContent = JsonSerializer.Serialize(adminTokenResponse);
        var adminTokenHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(adminTokenContent, Encoding.UTF8, "application/json")
        };

        // Simula requisição de cria��o de usu�rio
        var userCreationResponse = new HttpResponseMessage(HttpStatusCode.Created);

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(adminTokenHttpResponse) // First call for admin token
            .ReturnsAsync(userCreationResponse);  // Second call for user creation

        // Act
        var result = await _service.CreateUserAsync(email, username, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.UserId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var email = Email.Create("duplicate@example.com");
        var username = Username.Create("duplicateuser");
        var password = "SecurePassword123!";
        
        // Simula requisição de token de admin
        var adminTokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "admin-token",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        var adminTokenContent = JsonSerializer.Serialize(adminTokenResponse);
        var adminTokenHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(adminTokenContent, Encoding.UTF8, "application/json")
        };

        // Simula conflito na cria��o de usu�rio
        var conflictResponse = new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent("{\"errorMessage\":\"User exists with same email\"}", Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(adminTokenHttpResponse) // First call for admin token
            .ReturnsAsync(conflictResponse);      // Second call for user creation

        // Act
        var result = await _service.CreateUserAsync(email, username, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("User exists");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ShouldReturnValidResult()
    {
        // Arrange
        var accessToken = "valid-access-token";
        
        var userInfoResponse = new
        {
            sub = "user-id-123",
            email = "user@example.com",
            preferred_username = "testuser",
            realm_access = new { roles = new[] { "user", "admin" } }
        };

        var responseContent = JsonSerializer.Serialize(userInfoResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains("userinfo")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.ValidateTokenAsync(accessToken);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.UserId.Should().Be("user-id-123");
        result.Email.Should().Be("user@example.com");
        result.Username.Should().Be("testuser");
        result.Roles.Should().Contain("user");
        result.Roles.Should().Contain("admin");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ShouldReturnInvalidResult()
    {
        // Arrange
        var accessToken = "invalid-access-token";
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains("userinfo")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.ValidateTokenAsync(accessToken);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Token validation failed");
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidUserId_ShouldReturnSuccess()
    {
        // Arrange
        var userId = "user-to-delete-123";
        
        // Simula requisição de token de admin
        var adminTokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "admin-token",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        var adminTokenContent = JsonSerializer.Serialize(adminTokenResponse);
        var adminTokenHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(adminTokenContent, Encoding.UTF8, "application/json")
        };

        // Simula requisição de exclus�o de usu�rio
        var deletionResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(adminTokenHttpResponse) // First call for admin token
            .ReturnsAsync(deletionResponse);      // Second call for user deletion

        // Act
        var result = await _service.DeleteUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistentUserId_ShouldReturnFailure()
    {
        // Arrange
        var userId = "non-existent-user-123";
        
        // Simula requisição de token de admin
        var adminTokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "admin-token",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        var adminTokenContent = JsonSerializer.Serialize(adminTokenResponse);
        var adminTokenHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(adminTokenContent, Encoding.UTF8, "application/json")
        };

        // Simula usu�rio n�o encontrado
        var notFoundResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(adminTokenHttpResponse) // First call for admin token
            .ReturnsAsync(notFoundResponse);      // Second call for user deletion

        // Act
        var result = await _service.DeleteUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("User not found");
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var email = "existing@example.com";
        
        // Simula requisição de token de admin
        var adminTokenResponse = new KeycloakTokenResponse
        {
            AccessToken = "admin-token",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        var adminTokenContent = JsonSerializer.Serialize(adminTokenResponse);
        var adminTokenHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(adminTokenContent, Encoding.UTF8, "application/json")
        };

        // Mock user search result
        var userSearchResult = new[]
        {
            new
            {
                id = "user-123",
                email = email,
                username = "existinguser",
                enabled = true
            }
        };

        var searchContent = JsonSerializer.Serialize(userSearchResult);
        var searchResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(searchContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(adminTokenHttpResponse) // First call for admin token
            .ReturnsAsync(searchResponse);        // Second call for user search

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Id.Should().Be("user-123");
        result.User.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task AuthenticateAsync_WithInvalidInput_ShouldThrowArgumentException(string? invalidInput)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.AuthenticateAsync(invalidInput!, "password"));
        
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.AuthenticateAsync("username", invalidInput!));
    }

    [Fact]
    public async Task AuthenticateAsync_WithHttpRequestException_ShouldReturnFailureResult()
    {
        // Arrange
        var username = "testuser";
        var password = "testpass";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.AuthenticateAsync(username, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Network error");
    }

    [Fact]
    public async Task CreateUserAsync_WithNetworkError_ShouldReturnFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var username = Username.Create("testuser");
        var password = "SecurePassword123!";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _service.CreateUserAsync(email, username, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("timeout");
    }
}
