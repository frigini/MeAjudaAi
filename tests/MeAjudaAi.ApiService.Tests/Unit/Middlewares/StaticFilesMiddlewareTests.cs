using FluentAssertions;
using MeAjudaAi.ApiService.Middlewares;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.ApiService.Tests.Unit.Middlewares;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class StaticFilesMiddlewareTests
{
    [Theory]
    [InlineData("/css/style.css")]
    [InlineData("/js/app.js")]
    [InlineData("/images/logo.png")]
    [InlineData("/fonts/font.woff2")]
    public async Task InvokeAsync_WithStaticFilePath_ShouldCallNextMiddleware(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new StaticFilesMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Middleware should always pass through
        nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/swagger/index.html")]
    [InlineData("/health")]
    public async Task InvokeAsync_WithNonStaticPath_ShouldCallNextMiddleware(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new StaticFilesMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Middleware should always pass through
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldAlwaysCallNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/css/style.css";
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new StaticFilesMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Middleware should always pass through to next
        nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("/css/")]
    [InlineData("/js/")]
    [InlineData("/images/")]
    [InlineData("/fonts/")]
    public async Task InvokeAsync_WithStaticDirectoryPaths_ShouldCallNextMiddleware(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new StaticFilesMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyPath_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/";
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new StaticFilesMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("/CSS/style.css")]
    [InlineData("/JS/app.js")]
    [InlineData("/IMAGES/logo.png")]
    public async Task InvokeAsync_WithUppercaseStaticPaths_ShouldCallNextMiddleware(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new StaticFilesMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Path segments are case-sensitive, so uppercase won't match
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldAcceptRequestDelegate()
    {
        // Arrange
        RequestDelegate next = (ctx) => Task.CompletedTask;

        // Act
        var act = () => new StaticFilesMiddleware(next);

        // Assert
        act.Should().NotThrow();
    }
}
