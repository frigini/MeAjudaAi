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
using MeAjudaAi.Shared.Caching;
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

    [Theory]
    [InlineData("meajudaai-user-admin")]
    [InlineData("MEAJUDAAI-USER-ADMIN")]
    public void MapKeycloakRoleToPermissions_UserAdminRole_ShouldReturnUserAdminPermissions(string role)
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions(role);

        // Assert
        result.Should().Contain(EPermission.AdminUsers);
        result.Should().Contain(EPermission.UsersRead);
        result.Should().Contain(EPermission.UsersCreate);
        result.Should().Contain(EPermission.UsersUpdate);
        result.Should().Contain(EPermission.UsersList);
    }

    [Theory]
    [InlineData("meajudaai-user-operator")]
    [InlineData("MEAJUDAAI-USER-OPERATOR")]
    public void MapKeycloakRoleToPermissions_UserOperatorRole_ShouldReturnOperatorPermissions(string role)
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions(role);

        // Assert
        result.Should().Contain(EPermission.UsersRead);
        result.Should().Contain(EPermission.UsersUpdate);
        result.Should().Contain(EPermission.UsersList);
        result.Should().NotContain(EPermission.UsersCreate);
    }

    [Theory]
    [InlineData("meajudaai-provider-admin")]
    [InlineData("MEAJUDAAI-PROVIDER-ADMIN")]
    public void MapKeycloakRoleToPermissions_ProviderAdminRole_ShouldReturnProviderAdminPermissions(string role)
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions(role);

        // Assert
        result.Should().Contain(EPermission.ProvidersRead);
        result.Should().Contain(EPermission.ProvidersCreate);
        result.Should().Contain(EPermission.ProvidersUpdate);
        result.Should().Contain(EPermission.ProvidersDelete);
    }

    [Theory]
    [InlineData("meajudaai-provider")]
    [InlineData("MEAJUDAAI-PROVIDER")]
    public void MapKeycloakRoleToPermissions_ProviderRole_ShouldReturnLimitedPermissions(string role)
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions(role);

        // Assert
        result.Should().Contain(EPermission.ProvidersRead);
        result.Should().NotContain(EPermission.ProvidersCreate);
    }

    [Theory]
    [InlineData("meajudaai-order-admin")]
    [InlineData("MEAJUDAAI-ORDER-ADMIN")]
    public void MapKeycloakRoleToPermissions_OrderAdminRole_ShouldReturnOrderAdminPermissions(string role)
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions(role);

        // Assert
        result.Should().Contain(EPermission.OrdersRead);
        result.Should().Contain(EPermission.OrdersCreate);
        result.Should().Contain(EPermission.OrdersUpdate);
        result.Should().Contain(EPermission.OrdersDelete);
    }

    [Theory]
    [InlineData("meajudaai-order-operator")]
    [InlineData("MEAJUDAAI-ORDER-OPERATOR")]
    public void MapKeycloakRoleToPermissions_OrderOperatorRole_ShouldReturnOperatorPermissions(string role)
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions(role);

        // Assert
        result.Should().Contain(EPermission.OrdersRead);
        result.Should().Contain(EPermission.OrdersUpdate);
        result.Should().NotContain(EPermission.OrdersCreate);
    }

    [Theory]
    [InlineData("meajudaai-report-admin")]
    [InlineData("MEAJUDAAI-REPORT-ADMIN")]
    public void MapKeycloakRoleToPermissions_ReportAdminRole_ShouldReturnReportPermissions(string role)
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions(role);

        // Assert
        result.Should().Contain(EPermission.ReportsView);
        result.Should().Contain(EPermission.ReportsExport);
        result.Should().Contain(EPermission.ReportsCreate);
    }

    [Theory]
    [InlineData("meajudaai-report-viewer")]
    [InlineData("MEAJUDAAI-REPORT-VIEWER")]
    public void MapKeycloakRoleToPermissions_ReportViewerRole_ShouldReturnViewerPermissions(string role)
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions(role);

        // Assert
        result.Should().Contain(EPermission.ReportsView);
        result.Should().NotContain(EPermission.ReportsExport);
    }

    [Theory]
    [InlineData("meajudaai-catalog-manager")]
    [InlineData("MEAJUDAAI-CATALOG-MANAGER")]
    public void MapKeycloakRoleToPermissions_CatalogManagerRole_ShouldReturnCatalogPermissions(string role)
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions(role);

        // Assert
        result.Should().Contain(EPermission.ServiceCatalogsRead);
        result.Should().Contain(EPermission.ServiceCatalogsManage);
    }

    [Theory]
    [InlineData("meajudaai-location-manager")]
    [InlineData("MEAJUDAAI-LOCATION-MANAGER")]
    public void MapKeycloakRoleToPermissions_LocationManagerRole_ShouldReturnLocationPermissions(string role)
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions(role);

        // Assert
        result.Should().Contain(EPermission.LocationsRead);
        result.Should().Contain(EPermission.LocationsManage);
    }

    [Fact]
    public void MapKeycloakRoleToPermissions_UnknownRole_ShouldReturnEmptyPermissions()
    {
        // Act
        var result = _resolver.MapKeycloakRoleToPermissions("unknown_role_123");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void MapKeycloakRoleToPermissions_NullRole_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => _resolver.MapKeycloakRoleToPermissions(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MapKeycloakRoleToPermissions_EmptyRole_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => _resolver.MapKeycloakRoleToPermissions("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task ResolvePermissionsAsync_WithNullUserId_ShouldThrowArgumentNullException()
    {
        // Act & Assert - the code throws ArgumentNullException
        var act = () => _resolver.ResolvePermissionsAsync((UserId?)null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ResolvePermissionsAsync_WithEmptyUserId_ShouldReturnEmptyList()
    {
        // Act
        var result = await _resolver.ResolvePermissionsAsync("   ");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolvePermissionsAsync_WithUserNotFound_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = "nonexistent-user";

        SetupHttpMessage(HttpMethod.Post, "token", new { access_token = "token" });
        SetupNotFoundHttpMessage(HttpMethod.Get, $"users/{userId}");

        // Act
        var result = await _resolver.ResolvePermissionsAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolvePermissionsAsync_WithHttpException_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = "user-123";

        SetupHttpMessage(HttpMethod.Post, "token", new { access_token = "token" });
        SetupHttpMessage(HttpMethod.Get, $"users/{userId}", new { id = "keycloak-id" });
        SetupThrowingHttpMessage(HttpMethod.Get, "role-mappings/realm", new HttpRequestException("Connection failed"));

        // Act
        var result = await _resolver.ResolvePermissionsAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CanResolve_Always_ReturnsTrue()
    {
        // Act
        var result = _resolver.CanResolve(EPermission.AdminSystem);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ModuleName_ShouldReturnUsersModule()
    {
        // Act
        var result = _resolver.ModuleName;

        // Assert
        result.Should().Be("Users");
    }

    private void SetupNotFoundHttpMessage(HttpMethod method, string pathPart)
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == method && req.RequestUri!.ToString().Contains(pathPart)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });
    }

    private void SetupThrowingHttpMessage(HttpMethod method, string pathPart, Exception exception)
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == method && req.RequestUri!.ToString().Contains(pathPart)),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);
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
