using FluentAssertions;
using MeAjudaAi.Gateway.Middlewares;
using MeAjudaAi.Gateway.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Gateway.Tests.Unit.Middleware;

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class EdgeAuthGuardMiddlewareTests
{
    private readonly Mock<ILogger<EdgeAuthGuardMiddleware>> _loggerMock;
    private readonly EdgeAuthGuardOptions _options;

    public EdgeAuthGuardMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<EdgeAuthGuardMiddleware>>();
        _options = new EdgeAuthGuardOptions
        {
            Enabled = true,
            PublicRoutes = ["/health", "/swagger", "/api/v1/auth/login"]
        };
    }

    private EdgeAuthGuardMiddleware CreateMiddleware()
    {
        return new EdgeAuthGuardMiddleware(
            _ => Task.CompletedTask,
            Microsoft.Extensions.Options.Options.Create(_options),
            _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenDisabled_ShouldCallNext()
    {
        var options = new EdgeAuthGuardOptions { Enabled = false };
        var middleware = new EdgeAuthGuardMiddleware(
            _ => Task.CompletedTask,
            Microsoft.Extensions.Options.Options.Create(options),
            _loggerMock.Object);

        var context = new DefaultHttpContext();
        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_PublicRoute_ShouldCallNext()
    {
        var middleware = CreateMiddleware();

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_PublicRouteWithSwagger_ShouldCallNext()
    {
        var middleware = CreateMiddleware();

        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_NonPublicRoute_Unauthenticated_ShouldReturn401()
    {
        var middleware = CreateMiddleware();

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/providers";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.Headers["X-Gateway-Challenge"].FirstOrDefault().Should().Be("true");
    }

    [Fact]
    public async Task InvokeAsync_NonPublicRoute_Authenticated_ShouldCallNext()
    {
        var middleware = CreateMiddleware();

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/providers";
        
        var identity = new System.Security.Claims.ClaimsIdentity("TestAuth");
        var claims = new System.Security.Claims.ClaimsPrincipal(identity);
        context.User = claims;

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
        context.Response.Headers["X-Gateway-Authenticated"].FirstOrDefault().Should().Be("true");
    }

    [Fact]
    public async Task InvokeAsync_PublicRoute_Authenticated_ShouldCallNext()
    {
        var middleware = CreateMiddleware();

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/auth/login";
        
        var identity = new System.Security.Claims.ClaimsIdentity("TestAuth");
        var claims = new System.Security.Claims.ClaimsPrincipal(identity);
        context.User = claims;

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }
}