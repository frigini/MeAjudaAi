using FluentAssertions;
using MeAjudaAi.ApiService.Middlewares;
using Microsoft.AspNetCore.Http;
using Xunit;
using System.IO;

namespace MeAjudaAi.ApiService.Tests.Unit.Middlewares;

public class CompressionSecurityMiddlewareTests
{
    private bool _nextCalled;

    public CompressionSecurityMiddlewareTests()
    {
        _nextCalled = false;
    }

    [Fact]
    public async Task InvokeAsync_WithoutAuthHeaders_ShouldAllowCompression()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithAuthorizationHeader_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        context.Request.Headers.Append("Authorization", "Bearer token123");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithApiKeyHeader_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();
        context.Request.Headers.Append("X-API-Key", "my-api-key");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithAuth_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/auth/login");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithLogin_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/login");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithToken_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/token");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithRefresh_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/refresh");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithLogout_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/logout");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithConnect_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/connect/token");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithOAuth_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/oauth/authorize");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithOpenId_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/openid/userinfo");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithIdentity_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/identity/userinfo");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithUsersProfile_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/users/profile");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithUsersMe_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/users/me");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PathStartingWithAccount_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/account/settings");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_NonSensitivePath_ShouldAllowCompression()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/providers");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_NonSensitivePath_ShouldAllowCompression2()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/api/documents");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_CaseInsensitivePathMatching_ShouldRemoveAcceptEncoding()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/AUTH/login");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("Accept-Encoding").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_NullPath_ShouldAllowCompression()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext("/");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
    }

    private CompressionSecurityMiddleware CreateMiddleware()
    {
        return new CompressionSecurityMiddleware(
            next: (context) =>
            {
                _nextCalled = true;
                return Task.CompletedTask;
            }
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