using FluentAssertions;
using MeAjudaAi.ApiService.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;

namespace MeAjudaAi.ApiService.Tests.Unit.Middlewares;

public class ContentSecurityPolicyMiddlewareTests
{
    private readonly Mock<ILogger<ContentSecurityPolicyMiddleware>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private bool _nextCalled;

    public ContentSecurityPolicyMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ContentSecurityPolicyMiddleware>>();
        _configurationMock = new Mock<IConfiguration>();
        _environmentMock = new Mock<IWebHostEnvironment>();
        _nextCalled = false;
    }

    [Fact]
    public async Task InvokeAsync_InDevelopmentEnvironment_ShouldAllowHttpsInCsp()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Development");
        _configurationMock.Setup(x => x["ContentSecurityPolicy:AllowSelfHosted"]).Returns("false");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Content-Security-Policy");
        context.Response.Headers["Content-Security-Policy"].ToString().Should().Contain("localhost");
    }

    [Fact]
    public async Task InvokeAsync_InProductionEnvironment_ShouldNotAllowHttpInCsp()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["ContentSecurityPolicy:AllowSelfHosted"]).Returns("false");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("https:");
        csp.Should().NotContain("http://localhost");
    }

    [Fact]
    public async Task InvokeAsync_Always_ShouldAddXContentTypeOptionsHeader()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["ContentSecurityPolicy:AllowSelfHosted"]).Returns("false");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Content-Type-Options");
        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
    }

    [Fact]
    public async Task InvokeAsync_Always_ShouldAddXFrameOptionsHeader()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["ContentSecurityPolicy:AllowSelfHosted"]).Returns("false");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Frame-Options");
        context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
    }

    [Fact]
    public async Task InvokeAsync_Always_ShouldAddXXSSProtectionHeader()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["ContentSecurityPolicy:AllowSelfHosted"]).Returns("false");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-XSS-Protection");
        context.Response.Headers["X-XSS-Protection"].ToString().Should().Be("1; mode=block");
    }

    [Fact]
    public async Task InvokeAsync_Always_ShouldAddReferrerPolicyHeader()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["ContentSecurityPolicy:AllowSelfHosted"]).Returns("false");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Referrer-Policy");
        context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task InvokeAsync_Always_ShouldAddPermissionsPolicyHeader()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["ContentSecurityPolicy:AllowSelfHosted"]).Returns("false");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Permissions-Policy");
        context.Response.Headers["Permissions-Policy"].ToString().Should().Contain("geolocation=()");
        context.Response.Headers["Permissions-Policy"].ToString().Should().Contain("microphone=()");
        context.Response.Headers["Permissions-Policy"].ToString().Should().Contain("camera=()");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextDelegate()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["ContentSecurityPolicy:AllowSelfHosted"]).Returns("false");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithSelfHostedEnabled_ShouldAllowHttpsInProduction()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["ContentSecurityPolicy:AllowSelfHosted"]).Returns("true");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("https:");
    }

    [Fact]
    public async Task InvokeAsync_CspPolicy_ShouldContainDefaultDirectives()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["ContentSecurityPolicy:AllowSelfHosted"]).Returns("false");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();

        // Should contain basic CSP directives
        csp.Should().Contain("default-src");
        csp.Should().Contain("script-src");
        csp.Should().Contain("style-src");
        csp.Should().Contain("img-src");
    }

    [Fact]
    public async Task InvokeAsync_ProductionWithKeycloakAuthority_ShouldIncludeAuthorityInCsp()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["Keycloak:Authority"]).Returns("https://keycloak.example.com/realms/myrealm");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("https://keycloak.example.com/realms/myrealm");
    }

    [Fact]
    public async Task InvokeAsync_ProductionWithBaseUrlAndRealm_ShouldBuildKeycloakUrl()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["Keycloak:Authority"]).Returns((string?)null);
        _configurationMock.Setup(x => x["Keycloak:BaseUrl"]).Returns("https://keycloak.example.com");
        _configurationMock.Setup(x => x["Keycloak:Realm"]).Returns("meajudaai");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("https://keycloak.example.com/realms/meajudaai");
    }

    [Fact]
    public async Task InvokeAsync_ProductionWithNoKeycloakConfig_ShouldNotIncludeKeycloakInCsp()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["Keycloak:Authority"]).Returns((string?)null);
        _configurationMock.Setup(x => x["Keycloak:BaseUrl"]).Returns((string?)null);
        _configurationMock.Setup(x => x["Keycloak:Realm"]).Returns((string?)null);

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        // connect-src should only contain 'self' (no keycloak URL)
        csp.Should().Contain("connect-src 'self'");
    }

    [Fact]
    public async Task InvokeAsync_ProductionWithBaseUrlTrailingSlash_ShouldBuildCorrectUrl()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["Keycloak:Authority"]).Returns((string?)null);
        _configurationMock.Setup(x => x["Keycloak:BaseUrl"]).Returns("https://keycloak.example.com/");
        _configurationMock.Setup(x => x["Keycloak:Realm"]).Returns("testrealm");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("https://keycloak.example.com/realms/testrealm");
        csp.Should().NotContain("https://keycloak.example.com//realms/testrealm");
    }

    [Fact]
    public async Task InvokeAsync_ProductionWithApiBaseUrl_ShouldIncludeApiUrlInCsp()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["ApiBaseUrl"]).Returns("https://api.example.com");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("https://api.example.com");
    }

    [Fact]
    public async Task InvokeAsync_ProductionWithWebSocketUrl_ShouldIncludeWebSocketInCsp()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _configurationMock.Setup(x => x["WebSocketUrl"]).Returns("wss://ws.example.com");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("wss://ws.example.com");
    }

    [Fact]
    public async Task InvokeAsync_ProductionEnvironment_ShouldContainUpgradeInsecureRequests()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("upgrade-insecure-requests");
    }

    [Fact]
    public async Task InvokeAsync_DevelopmentEnvironment_ShouldNotContainUpgradeInsecureRequests()
    {
        // Arrange
        _environmentMock.SetupGet(x => x.EnvironmentName).Returns("Development");

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().NotContain("upgrade-insecure-requests");
    }

    private ContentSecurityPolicyMiddleware CreateMiddleware()
    {
        return new ContentSecurityPolicyMiddleware(
            next: (context) =>
            {
                _nextCalled = true;
                return Task.CompletedTask;
            },
            logger: _loggerMock.Object,
            configuration: _configurationMock.Object,
            environment: _environmentMock.Object
        );
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();
        return context;
    }
}