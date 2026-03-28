using System.Net;
using System.Security.Claims;
using System.Text.Json;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Keycloak;
using MeAjudaAi.Shared.Authorization.ValueObjects;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Keycloak;

[Trait("Category", "Unit")]
public class KeycloakPermissionResolverTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<KeycloakPermissionResolver>> _loggerMock;
    private readonly KeycloakPermissionResolver _resolver;

    public KeycloakPermissionResolverTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object);
        _configurationMock = new Mock<IConfiguration>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<KeycloakPermissionResolver>>();

        // Setup Keycloak config
        var configDict = new Dictionary<string, string?>
        {
            {"Keycloak:BaseUrl", "http://auth"},
            {"Keycloak:Realm", "test"},
            {"Keycloak:AdminClientId", "admin"},
            {"Keycloak:AdminClientSecret", "secret"}
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();

        _resolver = new KeycloakPermissionResolver(
            _httpClient,
            configuration,
            _cacheMock.Object,
            _loggerMock.Object);

        // Setup cache mock to always execute the factory
        _cacheMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<string>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (string key, Func<CancellationToken, ValueTask<IReadOnlyList<string>>> factory, TimeSpan? expiration, HybridCacheEntryOptions? opt, IReadOnlyCollection<string>? tags, CancellationToken ct) => 
                await factory(ct));

        _cacheMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<string>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (string key, Func<CancellationToken, ValueTask<string>> factory, TimeSpan? expiration, HybridCacheEntryOptions? opt, IReadOnlyCollection<string>? tags, CancellationToken ct) => 
                await factory(ct));
    }

    [Fact]
    public void MapKeycloakRoleToPermissions_ForAdminRole_ShouldReturnAllPermissions()
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions("admin");

        // Assert
        result.Should().Contain(EPermission.AdminSystem);
        result.Should().Contain(EPermission.UsersRead);
        result.Should().Contain(EPermission.ServiceCatalogsManage);
    }

    [Fact]
    public async Task ResolvePermissionsAsync_WithValidUser_ShouldReturnPermissions()
    {
        // Arrange
        var userId = "user-123";
        
        // Mock token response
        SetupHttpMessage(HttpMethod.Post, "token", new { access_token = "token" });
        
        // Mock user search response (by ID)
        SetupHttpMessage(HttpMethod.Get, $"users/{userId}", new { id = "keycloak-id", username = userId });
        
        // Mock role mappings response
        SetupHttpMessage(HttpMethod.Get, "role-mappings/realm", new[] { new { name = "admin" } });

        // Act
        var result = await _resolver.ResolvePermissionsAsync(userId);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(EPermission.AdminSystem);
    }

    private void SetupHttpMessage(HttpMethod method, string pathPart, object responseBody)
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == method && req.RequestUri!.ToString().Contains(pathPart)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseBody))
            });
    }
}
