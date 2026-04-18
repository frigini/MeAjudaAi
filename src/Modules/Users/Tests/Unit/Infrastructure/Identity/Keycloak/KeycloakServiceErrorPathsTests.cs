using System.Net;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Identity.Keycloak;

public class KeycloakServiceErrorPathsTests
{
    private readonly KeycloakOptions _options = new()
    {
        BaseUrl = "http://keycloak",
        Realm = "test",
        AdminUsername = "admin",
        AdminPassword = "password"
    };
    private readonly Mock<ILogger<KeycloakService>> _loggerMock = new();
    private readonly Mock<HttpMessageHandler> _handlerMock = new();

    [Fact]
    public async Task GetAdminTokenAsync_ShouldReturnFailure_WhenTokenEndpointReturns401()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("protocol/openid-connect/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Invalid credentials")
            });

        var httpClient = new HttpClient(_handlerMock.Object);
        var service = new KeycloakService(httpClient, _options, _loggerMock.Object);

        // Act
        var result = await service.CreateUserAsync("u", "e", "f", "l", "p", []);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to authenticate admin user");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnFailure_WhenPayloadIsInvalid()
    {
        // Arrange
        // Mock Admin Token (Success)
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("protocol/openid-connect/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"admin-token\",\"expires_in\":3600}")
            });

        // Mock Create User (Failure)
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("admin/realms/test/users")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Invalid user data")
            });

        var httpClient = new HttpClient(_handlerMock.Object);
        var service = new KeycloakService(httpClient, _options, _loggerMock.Object);

        // Act
        var result = await service.CreateUserAsync("u", "e", "f", "l", "p", []);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to create user in Keycloak");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldSucceed_WhenRolesAreEmpty()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("protocol/openid-connect/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"admin-token\",\"expires_in\":3600}")
            });

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("admin/realms/test/users")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Headers = { Location = new Uri("http://keycloak/admin/realms/test/users/user-guid") }
            });

        var httpClient = new HttpClient(_handlerMock.Object);
        var service = new KeycloakService(httpClient, _options, _loggerMock.Object);

        // Act
        var result = await service.CreateUserAsync("u", "e", "f", "l", "p", []);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("user-guid");
        
        // Verify no call to assign roles
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("role-mappings")),
            ItExpr.IsAny<CancellationToken>());
    }
}
