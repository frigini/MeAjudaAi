using System.Net;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.ApiService.Middlewares;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.ApiService.Options.RateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Middlewares;

public class RateLimitingMiddlewareTests
{
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<RateLimitingMiddleware>> _loggerMock;
    private readonly Mock<IOptionsMonitor<RateLimitOptions>> _optionsMock;
    private bool _nextCalled;

    public RateLimitingMiddlewareTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
        _optionsMock = new Mock<IOptionsMonitor<RateLimitOptions>>();
        _nextCalled = false;
    }

    [Fact]
    public async Task InvokeAsync_WhenRateLimitingDisabled_ShouldBypass()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.Enabled = false;
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WhenIpIsWhitelisted_ShouldBypass()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.EnableIpWhitelist = true;
        options.General.WhitelistedIps = ["127.0.0.1"];
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();
        var context = CreateHttpContext(remoteIp: "127.0.0.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WhenLimitNotExceeded_ShouldAllowRequest()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.WindowInSeconds = 60;
        options.Anonymous.RequestsPerMinute = 10;
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WhenLimitExceeded_ShouldReturn429()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.WindowInSeconds = 60;
        options.Anonymous.RequestsPerMinute = 2;
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act - Make 3 requests (limit is 2)
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(); // Reset context
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(); // Reset context
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
        context.Response.Headers.Should().ContainKey("Retry-After");

        var responseBody = await ReadResponseBody(context);
        responseBody.Should().Contain("RateLimitExceeded");
    }

    [Fact]
    public async Task InvokeAsync_WhenApproachingLimit_ShouldLogInformation()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.WindowInSeconds = 60;
        options.Anonymous.RequestsPerMinute = 10;
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act - Make 8 requests (80% of 10)
        for (int i = 0; i < 8; i++)
        {
            context = CreateHttpContext();
            await middleware.InvokeAsync(context);
        }

        // Assert - Should log information about approaching limit
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("approaching rate limit")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_ShouldUseAuthenticatedLimits()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.WindowInSeconds = 60;
        options.Anonymous.RequestsPerMinute = 2;
        options.Authenticated.RequestsPerMinute = 100;
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();
        var context = CreateHttpContext(isAuthenticated: true);

        // Act - Make 3 requests (would exceed anonymous limit but not authenticated)
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(isAuthenticated: true);
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(isAuthenticated: true);
        await middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_EndpointSpecificLimit_ShouldApplyCorrectLimit()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.WindowInSeconds = 60;
        options.Anonymous.RequestsPerMinute = 100;
        options.EndpointLimits = new Dictionary<string, EndpointLimits>
        {
            ["api_search"] = new EndpointLimits
            {
                Pattern = "/api/search*",
                RequestsPerMinute = 2,
                RequestsPerHour = 100,
                ApplyToAnonymous = true,
                ApplyToAuthenticated = true
            }
        };
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();
        var context = CreateHttpContext(path: "/api/search");

        // Act - Make 3 requests to /api/search
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(path: "/api/search");
        await middleware.InvokeAsync(context);
        context = CreateHttpContext(path: "/api/search");
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_RoleBasedLimit_ShouldApplyRoleLimit()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.WindowInSeconds = 60;
        options.Authenticated.RequestsPerMinute = 10;
        options.RoleLimits = new Dictionary<string, RoleLimits>
        {
            ["premium"] = new RoleLimits
            {
                RequestsPerMinute = 1000,
                RequestsPerHour = 50000,
                RequestsPerDay = 500000
            }
        };
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();
        var context = CreateHttpContext(isAuthenticated: true, roles: ["premium"]);

        // Act - Make 20 requests (would exceed default auth limit but not premium)
        for (int i = 0; i < 20; i++)
        {
            context = CreateHttpContext(isAuthenticated: true, roles: ["premium"]);
            await middleware.InvokeAsync(context);
        }

        // Assert
        _nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_DifferentPaths_ShouldHaveSeparateCounters()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.WindowInSeconds = 60;
        options.Anonymous.RequestsPerMinute = 2;
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();

        // Act
        var context1 = CreateHttpContext(path: "/api/users");
        await middleware.InvokeAsync(context1);
        await middleware.InvokeAsync(CreateHttpContext(path: "/api/users"));

        var context2 = CreateHttpContext(path: "/api/providers");
        await middleware.InvokeAsync(context2);
        await middleware.InvokeAsync(CreateHttpContext(path: "/api/providers"));

        // Assert - Both paths should be at their limit
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);

        // Third request to each path should fail
        var context3 = CreateHttpContext(path: "/api/users");
        await middleware.InvokeAsync(context3);
        context3.Response.StatusCode.Should().Be(429);

        var context4 = CreateHttpContext(path: "/api/providers");
        await middleware.InvokeAsync(context4);
        context4.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_LimitExceeded_ShouldIncludeRetryAfterHeader()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.WindowInSeconds = 60;
        options.Anonymous.RequestsPerMinute = 1;
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);
        context = CreateHttpContext();
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
        context.Response.Headers.Should().ContainKey("Retry-After");

        var retryAfter = context.Response.Headers["Retry-After"].ToString();
        int.TryParse(retryAfter, out var seconds).Should().BeTrue();
        seconds.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(60);
    }

    [Fact]
    public async Task InvokeAsync_EndpointLimit_OnlyAnonymous_ShouldNotApplyToAuthenticated()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.WindowInSeconds = 60;
        options.Authenticated.RequestsPerMinute = 100;
        options.EndpointLimits = new Dictionary<string, EndpointLimits>
        {
            ["api_search"] = new EndpointLimits
            {
                Pattern = "/api/search*",
                RequestsPerMinute = 2,
                RequestsPerHour = 100,
                ApplyToAnonymous = true,
                ApplyToAuthenticated = false
            }
        };
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();

        // Act - Authenticated user should use default auth limit, not endpoint limit
        for (int i = 0; i < 10; i++)
        {
            var context = CreateHttpContext(path: "/api/search", isAuthenticated: true);
            await middleware.InvokeAsync(context);
        }

        // Assert - Should succeed (using 100/min limit, not 2/min endpoint limit)
        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_PatternCacheSizeLimit_ShouldCompileOnDemandWhenLimitReached()
    {
        // Arrange
        var options = CreateDefaultOptions();
        options.General.WindowInSeconds = 60;
        options.Anonymous.RequestsPerMinute = 10;
        
        // Create 1001 unique endpoint patterns to exceed MaxPatternCacheSize (1000)
        var endpointLimits = new Dictionary<string, EndpointLimits>();
        for (int i = 0; i < 1001; i++)
        {
            endpointLimits[$"pattern_{i}"] = new EndpointLimits
            {
                Pattern = $"/api/test{i}/*",
                RequestsPerMinute = 10,
                RequestsPerHour = 100,
                ApplyToAnonymous = true,
                ApplyToAuthenticated = true
            };
        }
        options.EndpointLimits = endpointLimits;
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = CreateMiddleware();

        // Act - Request paths that match patterns beyond cache limit
        // First 1000 patterns should be cached, pattern 1001 should be compiled on-demand
        var context1000 = CreateHttpContext(path: "/api/test999/data"); // Within cache
        var context1001 = CreateHttpContext(path: "/api/test1000/data"); // Beyond cache limit

        await middleware.InvokeAsync(context1000);
        await middleware.InvokeAsync(context1001);

        // Assert - Both should succeed (rate limit applied correctly even without caching)
        context1000.Response.StatusCode.Should().Be(200);
        context1001.Response.StatusCode.Should().Be(200);

        // Verify warning was logged when cache limit reached
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Pattern cache size limit reached")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    // Helper methods

    private RateLimitingMiddleware CreateMiddleware()
    {
        return new RateLimitingMiddleware(
            next: (context) =>
            {
                _nextCalled = true;
                return Task.CompletedTask;
            },
            cache: _cache,
            options: _optionsMock.Object,
            logger: _loggerMock.Object
        );
    }

    private static HttpContext CreateHttpContext(
        string path = "/api/test",
        string remoteIp = "192.168.1.1",
        bool isAuthenticated = false,
        string[]? roles = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = "GET";
        context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
        context.Response.Body = new MemoryStream();

        if (isAuthenticated)
        {
            var claims = new List<Claim>
            {
                new("sub", "user123"),
                new(ClaimTypes.Name, "testuser")
            };

            if (roles != null)
            {
                claims.AddRange(roles.Select(r => new Claim("role", r)));
            }

            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        return context;
    }

    private static RateLimitOptions CreateDefaultOptions()
    {
        return new RateLimitOptions
        {
            General = new GeneralSettings
            {
                Enabled = true,
                EnableIpWhitelist = false,
                WhitelistedIps = [],
                WindowInSeconds = 60,
                ErrorMessage = "Too many requests. Please try again later."
            },
            Anonymous = new AnonymousLimits
            {
                RequestsPerMinute = 60,
                RequestsPerHour = 1000,
                RequestsPerDay = 5000
            },
            Authenticated = new AuthenticatedLimits
            {
                RequestsPerMinute = 120,
                RequestsPerHour = 2000,
                RequestsPerDay = 10000
            },
            RoleLimits = new Dictionary<string, RoleLimits>(),
            EndpointLimits = new Dictionary<string, EndpointLimits>()
        };
    }

    private static async Task<string> ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
