using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Gateway.Middleware;
using MeAjudaAi.Gateway.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MeAjudaAi.Gateway.Tests.Unit.Middleware;

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class AuthenticationGuardOptionsTests
{
    [Fact]
    public void PublicRoutesOptions_DefaultValues_ShouldContainHealthRoute()
    {
        var options = new PublicRoutesOptions();

        options.Should().NotBeNull();
        options.Routes.Should().NotBeEmpty();
        options.Routes.Should().Contain("/health");
    }

    [Fact]
    public void PublicRoutesOptions_SectionName_ShouldBePublicRoutes()
    {
        PublicRoutesOptions.SectionName.Should().Be("PublicRoutes");
    }

    [Fact]
    public void PublicRoutesOptions_WithCustomRoutes_ShouldConfigureCorrectly()
    {
        var options = new PublicRoutesOptions
        {
            Routes = ["/health", "/api/providers/search", "/api/locations/cep"]
        };

        options.Routes.Should().HaveCount(3);
        options.Routes.Should().Contain("/api/providers/search");
    }
}

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class AuthenticationGuardMiddlewareBehaviorTests
{
    private readonly Mock<ILogger<AuthenticationGuardMiddleware>> _loggerMock;
    private readonly Mock<IOptionsMonitor<PublicRoutesOptions>> _optionsMock;

    public AuthenticationGuardMiddlewareBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<AuthenticationGuardMiddleware>>();
        _optionsMock = new Mock<IOptionsMonitor<PublicRoutesOptions>>();

        _optionsMock.Setup(x => x.CurrentValue).Returns(new PublicRoutesOptions
        {
            Routes = ["/health", "/api/locations/cep", "/api/providers/search"]
        });
    }

    private AuthenticationGuardMiddleware CreateMiddleware(RequestDelegate? next = null)
    {
        next ??= _ => Task.CompletedTask;
        return new AuthenticationGuardMiddleware(next, _optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_PublicRoute_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_PublicRouteSubPath_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        var context = new DefaultHttpContext();
        context.Request.Path = "/health/ready";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_PublicApiRoute_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/locations/cep/01310100";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_UnauthenticatedUser_Returns401()
    {
        var middleware = CreateMiddleware();

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/bookings";
        context.Response.Body = new System.IO.MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_UnauthenticatedUser_ReturnsJsonBody()
    {
        var middleware = CreateMiddleware();

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/bookings";
        context.Response.Body = new System.IO.MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_AuthenticatedUser_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/bookings";

        var claims = new List<Claim> { new("sub", "user123") };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_AuthenticatedUser_DoesNotReturn401()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/users/profile";

        var claims = new List<Claim> { new("sub", "user456") };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().NotBe(401);
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_CaseInsensitivePublicRouteMatch_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        var context = new DefaultHttpContext();
        context.Request.Path = "/HEALTH/live";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_RootPath_UnauthenticatedUser_Returns401()
    {
        var middleware = CreateMiddleware();

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/admin/users";
        context.Response.Body = new System.IO.MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
    }
}