using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak.Models;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Identity.Keycloak;

public class KeycloakServiceTokenConcurrencyTests
{
    [Fact]
    public async Task CreateUserAsync_ShouldRequestAdminTokenOnlyOnce_WhenCalledConcurrently()
    {
        // Arrange
        var options = new KeycloakOptions
        {
            BaseUrl = "http://keycloak",
            Realm = "test",
            AdminUsername = "admin",
            AdminPassword = "password"
        };
        var loggerMock = new Mock<ILogger<KeycloakService>>();
        
        var serializerMock = new Mock<ISerializer>();
        serializerMock
            .Setup(s => s.Serialize(It.IsAny<object>()))
            .Returns((object obj) => JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }));

        serializerMock
            .Setup(s => s.Deserialize<KeycloakTokenResponse>(It.IsAny<string>()))
            .Returns((string json) => JsonSerializer.Deserialize<KeycloakTokenResponse>(json));
        
        var handlerMock = new Mock<HttpMessageHandler>();
        var tokenCallCount = 0;

        // Mock Admin Token Response (with delay to force concurrency)
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("protocol/openid-connect/token")),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async (HttpRequestMessage req, CancellationToken ct) =>
            {
                Interlocked.Increment(ref tokenCallCount);
                await Task.Delay(200); // Simulate network latency
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"access_token\":\"admin-token\",\"expires_in\":3600}")
                };
            });

        // Mock Create User Response
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("admin/realms/test/users")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Headers = { Location = new Uri("http://keycloak/admin/realms/test/users/user-id") }
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new KeycloakService(httpClient, options, loggerMock.Object, serializerMock.Object);

        // Act
        // Call CreateUserAsync multiple times concurrently
        var tasks = Enumerable.Range(0, 5).Select(_ => 
            service.CreateUserAsync("user", "email", "first", "last", "pass", [])).ToList();

        await Task.WhenAll(tasks);

        // Assert
        tokenCallCount.Should().Be(1, "because SemaphoreSlim should prevent multiple concurrent token requests");
    }
}
