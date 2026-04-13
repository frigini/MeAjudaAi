using FluentAssertions;
using MeAjudaAi.ApiService.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;

namespace MeAjudaAi.ApiService.Tests.Unit.Middlewares;

public class CompressionSecurityMiddlewareTests
{
    private bool _nextCalled;
    private readonly Mock<ILogger<CompressionSecurityMiddleware>> _loggerMock;

    public CompressionSecurityMiddlewareTests()
    {
        _nextCalled = false;
        _loggerMock = new Mock<ILogger<CompressionSecurityMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_WithoutAuthHeaders_ShouldAllowCompression()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("gzip");
        context.Response.Headers.ContainsKey("X-Compression-Disabled").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithAuthorizationHeader_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        context.Request.Headers.Append("Authorization", "Bearer token123");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
        context.Response.Headers["X-Compression-Disabled"].Should().BeEquivalentTo("Security-Policy");
    }

    [Fact]
    public async Task InvokeAsync_WithApiKeyHeader_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        context.Request.Headers.Append("X-API-Key", "my-api-key");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
        context.Response.Headers["X-Compression-Disabled"].Should().BeEquivalentTo("Security-Policy");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithAuth_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/auth/login");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithLogin_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/login");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithToken_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/token");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithRefresh_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/refresh");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithLogout_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/logout");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithConnect_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/connect/token");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithOAuth_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/oauth/authorize");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithOpenId_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/openid/userinfo");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithIdentity_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/identity/userinfo");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithUsersProfile_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/users/profile");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithUsersMe_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/users/me");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithAccount_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/account/settings");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_NonSensitivePath_ShouldAllowCompression()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/providers");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("gzip");
    }

    [Fact]
    public async Task InvokeAsync_CaseInsensitivePathMatching_ShouldSetIdentityEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/AUTH/login");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("identity");
    }

    [Fact]
    public async Task InvokeAsync_NullPath_ShouldAllowCompression()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/");
        context.Request.Headers.Append("Accept-Encoding", "gzip");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers["Accept-Encoding"].Should().BeEquivalentTo("gzip");
    }

    private CompressionSecurityMiddleware CreateMiddleware()
    {
        return new CompressionSecurityMiddleware(
            next: (context) =>
            {
                _nextCalled = true;
                return Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );
    }

    private static HttpContext CreateHttpContext(string path = "/api/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();
        return context;
    }
}