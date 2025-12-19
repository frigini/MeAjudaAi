using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Middleware;
using MeAjudaAi.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Middleware;

/// <summary>
/// Testes para PermissionOptimizationMiddleware
/// </summary>
public class PermissionOptimizationMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<PermissionOptimizationMiddleware>> _loggerMock;
    private readonly PermissionOptimizationMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public PermissionOptimizationMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<PermissionOptimizationMiddleware>>();
        _middleware = new PermissionOptimizationMiddleware(_nextMock.Object, _loggerMock.Object);
        _httpContext = new DefaultHttpContext();
    }

    #region Public Endpoint Tests

    [Fact]
    public async Task InvokeAsync_PublicEndpoint_ShouldSkipOptimization()
    {
        // Arrange
        _httpContext.Request.Path = ApiEndpoints.System.Health;
        _httpContext.Request.Method = "GET";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        _httpContext.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData("/metrics")]
    [InlineData("/swagger")]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/logout")]
    [InlineData("/.well-known/openid-configuration")]
    public async Task InvokeAsync_VariousPublicEndpoints_ShouldSkipOptimization(string path)
    {
        // Arrange
        _httpContext.Request.Path = path;
        _httpContext.Request.Method = "GET";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        _httpContext.Items.Should().BeEmpty();
    }

    #endregion

    #region Unauthenticated User Tests

    [Fact]
    public async Task InvokeAsync_UnauthenticatedUser_ShouldSkipOptimization()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users";
        _httpContext.Request.Method = "GET";
        _httpContext.User = new ClaimsPrincipal(); // No identity

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        _httpContext.Items.Should().BeEmpty();
    }

    #endregion

    #region Authenticated User Tests

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_ShouldApplyOptimizations()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users";
        _httpContext.Request.Method = "GET";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim("tenant_id", "tenant-1"),
            new Claim("organization_id", "org-1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        _httpContext.Items.Should().ContainKey("UserId");
        _httpContext.Items["UserId"].Should().Be("user-123");
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_ShouldCacheUserContext()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/providers";
        _httpContext.Request.Method = "POST";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-456"),
            new Claim("tenant_id", "tenant-2"),
            new Claim("organization_id", "org-2"),
            new Claim("system_admin", "true")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items.Should().ContainKey("UserId");
        _httpContext.Items.Should().ContainKey("PermissionCacheTimestamp");
        _httpContext.Items["UserId"].Should().Be("user-456");
    }

    #endregion

    #region Permission Preloading Tests

    [Theory]
    [InlineData("/api/v1/users", "GET")]
    [InlineData("/api/v1/users/123", "PUT")]
    [InlineData("/api/v1/users/456", "DELETE")]
    public async Task InvokeAsync_UsersModule_ShouldPreloadExpectedPermissions(string path, string method)
    {
        // Arrange
        _httpContext.Request.Path = path;
        _httpContext.Request.Method = method;

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items.Should().ContainKey("ExpectedPermissions");
        var permissions = _httpContext.Items["ExpectedPermissions"] as List<EPermission>;
        permissions.Should().NotBeNull().And.NotBeEmpty();
    }

    [Theory]
    [InlineData("/api/v1/providers", "GET", EPermission.ProvidersRead)]
    [InlineData("/api/v1/providers", "POST", EPermission.ProvidersCreate)]
    [InlineData("/api/v1/providers/123", "PUT", EPermission.ProvidersUpdate)]
    [InlineData("/api/v1/providers/456", "DELETE", EPermission.ProvidersDelete)]
    public async Task InvokeAsync_ProvidersModule_ShouldIdentifyCorrectPermissions(
        string path, string method, EPermission expectedPermission)
    {
        // Arrange
        _httpContext.Request.Path = path;
        _httpContext.Request.Method = method;

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var permissions = _httpContext.Items["ExpectedPermissions"] as List<EPermission>;
        permissions.Should().Contain(expectedPermission);
    }

    [Fact]
    public async Task InvokeAsync_UserProfileEndpoint_ShouldIdentifyProfilePermission()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users/profile";
        _httpContext.Request.Method = "GET";

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var permissions = _httpContext.Items["ExpectedPermissions"] as List<EPermission>;
        permissions.Should().Contain(EPermission.UsersProfile);
    }

    [Fact]
    public async Task InvokeAsync_AdminUsersEndpoint_ShouldIdentifyAdminPermissions()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users/admin/list";
        _httpContext.Request.Method = "GET";

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var permissions = _httpContext.Items["ExpectedPermissions"] as List<EPermission>;
        permissions.Should().Contain(EPermission.AdminUsers);
        permissions.Should().Contain(EPermission.UsersList);
    }

    [Fact]
    public async Task InvokeAsync_UserDelete_ShouldRequireAdminPermission()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users/123";
        _httpContext.Request.Method = "DELETE";

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var permissions = _httpContext.Items["ExpectedPermissions"] as List<EPermission>;
        permissions.Should().Contain(EPermission.UsersDelete);
        permissions.Should().Contain(EPermission.AdminUsers);
    }

    #endregion

    #region ReadOnly Optimization Tests

    [Fact]
    public async Task InvokeAsync_ProfileGET_ShouldUseAggressiveCache()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users/profile";
        _httpContext.Request.Method = "GET";

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items["UseAggressivePermissionCache"].Should().Be(true);
        _httpContext.Items["PermissionCacheDuration"].Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task InvokeAsync_ProfileEndpointGET_ShouldUseAggressiveCache()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users/profile/123";
        _httpContext.Request.Method = "GET";

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items["UseAggressivePermissionCache"].Should().Be(true);
        _httpContext.Items["PermissionCacheDuration"].Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task InvokeAsync_GenericAPIGET_ShouldUseIntermediateCache()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/providers";
        _httpContext.Request.Method = "GET";

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items["UseAggressivePermissionCache"].Should().Be(false);
        _httpContext.Items["PermissionCacheDuration"].Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task InvokeAsync_POST_ShouldNotSetCacheOptimizations()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users";
        _httpContext.Request.Method = "POST";

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items.Should().NotContainKey("UseAggressivePermissionCache");
        _httpContext.Items.Should().NotContainKey("PermissionCacheDuration");
    }

    #endregion

    #region Admin Path Tests

    [Theory]
    [InlineData("/api/v1/users/admin/settings", EPermission.AdminUsers)]
    [InlineData("/api/v1/users/admin/config", EPermission.AdminUsers)]
    public async Task InvokeAsync_AdminPaths_ShouldRequireAdminPermission(string path, EPermission expectedPermission)
    {
        // Arrange
        _httpContext.Request.Path = path;
        _httpContext.Request.Method = "GET";

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var permissions = _httpContext.Items["ExpectedPermissions"] as List<EPermission>;
        permissions.Should().Contain(expectedPermission);
    }

    #endregion

    #region Future Modules Tests

    [Theory]
    [InlineData("/api/v1/orders", "GET", EPermission.OrdersRead)]
    [InlineData("/api/v1/orders", "POST", EPermission.OrdersCreate)]
    [InlineData("/api/v1/orders/123", "PUT", EPermission.OrdersUpdate)]
    [InlineData("/api/v1/orders/456", "DELETE", EPermission.OrdersDelete)]
    public async Task InvokeAsync_OrdersModule_ShouldIdentifyCorrectPermissions(
        string path, string method, EPermission expectedPermission)
    {
        // Arrange
        _httpContext.Request.Path = path;
        _httpContext.Request.Method = method;

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var permissions = _httpContext.Items["ExpectedPermissions"] as List<EPermission>;
        permissions.Should().Contain(expectedPermission);
    }

    [Theory]
    [InlineData("/api/v1/reports", "GET", EPermission.ReportsView)]
    [InlineData("/api/v1/reports", "POST", EPermission.ReportsCreate)]
    [InlineData("/api/v1/reports/export", "GET", EPermission.ReportsExport)]
    public async Task InvokeAsync_ReportsModule_ShouldIdentifyCorrectPermissions(
        string path, string method, EPermission expectedPermission)
    {
        // Arrange
        _httpContext.Request.Path = path;
        _httpContext.Request.Method = method;

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var permissions = _httpContext.Items["ExpectedPermissions"] as List<EPermission>;
        permissions.Should().Contain(expectedPermission);
    }

    #endregion

    #region Extension Methods Tests

    [Fact]
    public void GetExpectedPermissions_WithPermissions_ShouldReturnThem()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedPermissions = new List<EPermission> { EPermission.UsersRead, EPermission.UsersCreate };
        context.Items["ExpectedPermissions"] = expectedPermissions;

        // Act
        var result = context.GetExpectedPermissions();

        // Assert
        result.Should().BeEquivalentTo(expectedPermissions);
    }

    [Fact]
    public void GetExpectedPermissions_WithoutPermissions_ShouldReturnEmpty()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = context.GetExpectedPermissions();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldUseAggressivePermissionCache_WhenTrue_ShouldReturnTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items["UseAggressivePermissionCache"] = true;

        // Act
        var result = context.ShouldUseAggressivePermissionCache();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldUseAggressivePermissionCache_WhenFalse_ShouldReturnFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items["UseAggressivePermissionCache"] = false;

        // Act
        var result = context.ShouldUseAggressivePermissionCache();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldUseAggressivePermissionCache_WhenNotSet_ShouldReturnFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = context.ShouldUseAggressivePermissionCache();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetRecommendedPermissionCacheDuration_WithCustomDuration_ShouldReturnIt()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items["PermissionCacheDuration"] = TimeSpan.FromMinutes(25);

        // Act
        var result = context.GetRecommendedPermissionCacheDuration();

        // Assert
        result.Should().Be(TimeSpan.FromMinutes(25));
    }

    [Fact]
    public void GetRecommendedPermissionCacheDuration_WithoutCustomDuration_ShouldReturnDefault()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = context.GetRecommendedPermissionCacheDuration();

        // Assert
        result.Should().Be(TimeSpan.FromMinutes(15));
    }

    #endregion

    #region User ID Extraction Tests

    [Fact]
    public async Task InvokeAsync_UserIdFromNameIdentifier_ShouldExtractCorrectly()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users";
        _httpContext.Request.Method = "GET";

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-from-nameidentifier") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items["UserId"].Should().Be("user-from-nameidentifier");
    }

    [Fact]
    public async Task InvokeAsync_UserIdFromSub_ShouldExtractCorrectly()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users";
        _httpContext.Request.Method = "GET";

        var claims = new[] { new Claim("sub", "user-from-sub") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items["UserId"].Should().Be("user-from-sub");
    }

    [Fact]
    public async Task InvokeAsync_UserIdFromId_ShouldExtractCorrectly()
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users";
        _httpContext.Request.Method = "GET";

        var claims = new[] { new Claim("id", "user-from-id") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items["UserId"].Should().Be("user-from-id");
    }

    #endregion

    #region HTTP Methods Tests

    [Theory]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    public async Task InvokeAsync_ReadOnlyMethods_DoNotSetCacheDuration(string method)
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users";
        _httpContext.Request.Method = method;

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert - HEAD/OPTIONS are readonly but cache duration is only set for GET method
        _httpContext.Items.Should().ContainKey("UserId");
        _httpContext.Items.Should().NotContainKey("PermissionCacheDuration");
    }

    [Theory]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_UpdateMethods_ShouldIdentifyUpdatePermissions(string method)
    {
        // Arrange
        _httpContext.Request.Path = "/api/v1/users/123";
        _httpContext.Request.Method = method;

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var permissions = _httpContext.Items["ExpectedPermissions"] as List<EPermission>;
        permissions.Should().Contain(EPermission.UsersUpdate);
    }

    #endregion
}
