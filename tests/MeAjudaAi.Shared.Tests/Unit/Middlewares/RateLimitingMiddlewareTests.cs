using System.Net;
using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.ApiService.Middlewares;
using MeAjudaAi.ApiService.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Middlewares;

/// <summary>
/// Testes para RateLimitingMiddleware - sistema de rate limiting com suporte a usuários autenticados, IP whitelist, limites por endpoint e role.
/// </summary>
public class RateLimitingMiddlewareTests
{
    private readonly Mock<RequestDelegate> _next;
    private readonly Mock<ILogger<RateLimitingMiddleware>> _logger;
    private readonly Mock<IOptionsMonitor<RateLimitOptions>> _optionsMonitor;
    private RateLimitOptions _options;

    public RateLimitingMiddlewareTests()
    {
        _next = new Mock<RequestDelegate>();
        _logger = new Mock<ILogger<RateLimitingMiddleware>>();
        _optionsMonitor = new Mock<IOptionsMonitor<RateLimitOptions>>();

        // Default options
        _options = new RateLimitOptions
        {
            General = new GeneralSettings
            {
                Enabled = true,
                WindowInSeconds = 60,
                EnableIpWhitelist = false,
                WhitelistedIps = [],
                ErrorMessage = "Rate limit exceeded. Please try again later."
            },
            Anonymous = new AnonymousLimits
            {
                RequestsPerMinute = 30,
                RequestsPerHour = 300,
                RequestsPerDay = 1000
            },
            Authenticated = new AuthenticatedLimits
            {
                RequestsPerMinute = 120,
                RequestsPerHour = 2000,
                RequestsPerDay = 10000
            },
            EndpointLimits = new Dictionary<string, EndpointLimits>(),
            RoleLimits = new Dictionary<string, RoleLimits>()
        };

        _optionsMonitor.Setup(x => x.CurrentValue).Returns(_options);
    }

    #region Bypass Tests

    [Fact]
    public async Task InvokeAsync_WhenRateLimitingDisabled_ShouldBypass()
    {
        // Arrange
        _options.General.Enabled = false;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _next.Verify(x => x(context), Times.Once);
        context.Response.StatusCode.Should().Be(200); // default, not modified
    }

    [Fact]
    public async Task InvokeAsync_WhenIpInWhitelist_ShouldBypass()
    {
        // Arrange
        _options.General.EnableIpWhitelist = true;
        _options.General.WhitelistedIps = ["127.0.0.1"];
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context = CreateHttpContext(ip: "127.0.0.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _next.Verify(x => x(context), Times.Once);
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WhenIpNotInWhitelist_ShouldApplyRateLimit()
    {
        // Arrange
        _options.General.EnableIpWhitelist = true;
        _options.General.WhitelistedIps = ["192.168.1.100"];
        _options.Anonymous.RequestsPerMinute = 1; // Only 1 request allowed
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext(ip: "127.0.0.1");
        var context2 = CreateHttpContext(ip: "127.0.0.1");

        // Act
        await middleware.InvokeAsync(context1); // First request - OK
        await middleware.InvokeAsync(context2); // Second request - rate limited

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(429);
    }

    #endregion

    #region Anonymous User Tests

    [Fact]
    public async Task InvokeAsync_AnonymousUser_WithinLimit_ShouldAllow()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 5;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _next.Verify(x => x(context), Times.Once);
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_AnonymousUser_ExceedsLimit_ShouldReturn429()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 2;
        _options.General.WindowInSeconds = 60;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext(path: "/api/users");
        var context2 = CreateHttpContext(path: "/api/users");
        var context3 = CreateHttpContext(path: "/api/users");

        // Act
        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2
        await middleware.InvokeAsync(context3); // 3 - exceeds limit

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(429);
        context3.Response.Headers.Should().ContainKey("Retry-After");
        context3.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_AnonymousUser_DifferentPaths_ShouldHaveSeparateCounters()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 2;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext(path: "/api/users");
        var context2 = CreateHttpContext(path: "/api/users");
        var context3 = CreateHttpContext(path: "/api/providers"); // Different path

        // Act
        await middleware.InvokeAsync(context1); // /api/users: 1
        await middleware.InvokeAsync(context2); // /api/users: 2
        await middleware.InvokeAsync(context3); // /api/providers: 1 (separate counter)

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(200); // Different path, not rate limited
    }

    [Fact]
    public async Task InvokeAsync_AnonymousUser_DifferentMethods_ShouldHaveSeparateCounters()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 2;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext(method: "GET", path: "/api/users");
        var context2 = CreateHttpContext(method: "GET", path: "/api/users");
        var context3 = CreateHttpContext(method: "POST", path: "/api/users"); // Different method

        // Act
        await middleware.InvokeAsync(context1); // GET: 1
        await middleware.InvokeAsync(context2); // GET: 2
        await middleware.InvokeAsync(context3); // POST: 1 (separate counter)

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(200); // Different method, not rate limited
    }

    #endregion

    #region Authenticated User Tests

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_ShouldUseHigherLimit()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 2;
        _options.Authenticated.RequestsPerMinute = 5;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext(isAuthenticated: true, userId: "user1");
        var context2 = CreateHttpContext(isAuthenticated: true, userId: "user1");
        var context3 = CreateHttpContext(isAuthenticated: true, userId: "user1");

        // Act
        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2
        await middleware.InvokeAsync(context3); // 3 - would exceed anonymous limit (2), but OK for authenticated (5)

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(200); // Authenticated users have higher limits
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_ExceedsLimit_ShouldReturn429()
    {
        // Arrange
        _options.Authenticated.RequestsPerMinute = 2;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext(isAuthenticated: true, userId: "user1");
        var context2 = CreateHttpContext(isAuthenticated: true, userId: "user1");
        var context3 = CreateHttpContext(isAuthenticated: true, userId: "user1");

        // Act
        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2
        await middleware.InvokeAsync(context3); // 3 - exceeds

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_DifferentAuthenticatedUsers_ShouldHaveSeparateCounters()
    {
        // Arrange
        _options.Authenticated.RequestsPerMinute = 2;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext(isAuthenticated: true, userId: "user1");
        var context2 = CreateHttpContext(isAuthenticated: true, userId: "user1");
        var context3 = CreateHttpContext(isAuthenticated: true, userId: "user2"); // Different user

        // Act
        await middleware.InvokeAsync(context1); // user1: 1
        await middleware.InvokeAsync(context2); // user1: 2
        await middleware.InvokeAsync(context3); // user2: 1 (separate counter)

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(200); // Different user, not rate limited
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_UserKeyFallback_ShouldUseNameThenIp()
    {
        // Arrange
        _options.Authenticated.RequestsPerMinute = 2;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // User without "sub" claim but with Name
        var context1 = CreateHttpContext(isAuthenticated: true, userId: null, userName: "john.doe");
        var context2 = CreateHttpContext(isAuthenticated: true, userId: null, userName: "john.doe");
        var context3 = CreateHttpContext(isAuthenticated: true, userId: null, userName: "john.doe");

        // Act
        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2
        await middleware.InvokeAsync(context3); // 3 - exceeds

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(429); // Same Name, rate limited
    }

    #endregion

    #region Endpoint-Specific Limits

    [Fact]
    public async Task InvokeAsync_EndpointSpecificLimit_ExactMatch_ShouldApply()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 10;
        _options.EndpointLimits = new Dictionary<string, EndpointLimits>
        {
            ["api-status"] = new EndpointLimits
            {
                Pattern = "/api/status",
                RequestsPerMinute = 2,
                RequestsPerHour = 100,
                ApplyToAnonymous = true,
                ApplyToAuthenticated = true
            }
        };
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext(path: "/api/status");
        var context2 = CreateHttpContext(path: "/api/status");
        var context3 = CreateHttpContext(path: "/api/status");

        // Act
        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2
        await middleware.InvokeAsync(context3); // 3 - exceeds endpoint limit (2)

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(429); // Endpoint-specific limit is lower than default
    }

    [Fact]
    public async Task InvokeAsync_EndpointSpecificLimit_WildcardMatch_ShouldApply()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 10;
        _options.EndpointLimits = new Dictionary<string, EndpointLimits>
        {
            ["api-all"] = new EndpointLimits
            {
                Pattern = "/api/*",
                RequestsPerMinute = 3,
                RequestsPerHour = 100,
                ApplyToAnonymous = true,
                ApplyToAuthenticated = true
            }
        };
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext(path: "/api/users");
        var context2 = CreateHttpContext(path: "/api/users");
        var context3 = CreateHttpContext(path: "/api/users");
        var context4 = CreateHttpContext(path: "/api/users");

        // Act
        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2
        await middleware.InvokeAsync(context3); // 3
        await middleware.InvokeAsync(context4); // 4 - exceeds

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(200);
        context4.Response.StatusCode.Should().Be(429); // Wildcard pattern matched
    }

    [Fact]
    public async Task InvokeAsync_EndpointSpecificLimit_OnlyForAnonymous_AuthenticatedShouldUseDefault()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 10;
        _options.Authenticated.RequestsPerMinute = 20;
        _options.EndpointLimits = new Dictionary<string, EndpointLimits>
        {
            ["restricted"] = new EndpointLimits
            {
                Pattern = "/api/public",
                RequestsPerMinute = 2,
                RequestsPerHour = 100,
                ApplyToAnonymous = true,
                ApplyToAuthenticated = false // Authenticated users use default
            }
        };
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // Anonymous user - should hit endpoint limit (2)
        var anonContext1 = CreateHttpContext(path: "/api/public");
        var anonContext2 = CreateHttpContext(path: "/api/public");
        var anonContext3 = CreateHttpContext(path: "/api/public");

        // Authenticated user - should use default limit (20)
        var authContext1 = CreateHttpContext(isAuthenticated: true, userId: "user1", path: "/api/public");
        var authContext2 = CreateHttpContext(isAuthenticated: true, userId: "user1", path: "/api/public");
        var authContext3 = CreateHttpContext(isAuthenticated: true, userId: "user1", path: "/api/public");

        // Act
        await middleware.InvokeAsync(anonContext1); // 1
        await middleware.InvokeAsync(anonContext2); // 2
        await middleware.InvokeAsync(anonContext3); // 3 - exceeds
        await middleware.InvokeAsync(authContext1); // 1
        await middleware.InvokeAsync(authContext2); // 2
        await middleware.InvokeAsync(authContext3); // 3 - OK (uses default 20)

        // Assert
        anonContext1.Response.StatusCode.Should().Be(200);
        anonContext2.Response.StatusCode.Should().Be(200);
        anonContext3.Response.StatusCode.Should().Be(429); // Endpoint limit applied
        authContext1.Response.StatusCode.Should().Be(200);
        authContext2.Response.StatusCode.Should().Be(200);
        authContext3.Response.StatusCode.Should().Be(200); // Default limit (higher)
    }

    #endregion

    #region Role-Specific Limits

    [Fact]
    public async Task InvokeAsync_RoleSpecificLimit_AdminRole_ShouldApply()
    {
        // Arrange
        _options.Authenticated.RequestsPerMinute = 10;
        _options.RoleLimits = new Dictionary<string, RoleLimits>
        {
            ["Admin"] = new RoleLimits
            {
                RequestsPerMinute = 100,
                RequestsPerHour = 5000,
                RequestsPerDay = 50000
            }
        };
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // Admin user - 12 requests (exceeds default 10, but OK for admin 100)
        var contexts = Enumerable.Range(1, 12)
            .Select(_ => CreateHttpContext(isAuthenticated: true, userId: "admin1", roles: ["Admin"]))
            .ToList();

        // Act
        foreach (var context in contexts)
        {
            await middleware.InvokeAsync(context);
        }

        // Assert
        contexts.Should().AllSatisfy(c => c.Response.StatusCode.Should().Be(200)); // All requests OK (admin has 100/min)
    }

    [Fact]
    public async Task InvokeAsync_RoleSpecificLimit_UserWithMultipleRoles_ShouldUseFirstMatch()
    {
        // Arrange
        _options.Authenticated.RequestsPerMinute = 10;
        _options.RoleLimits = new Dictionary<string, RoleLimits>
        {
            ["Admin"] = new RoleLimits { RequestsPerMinute = 100, RequestsPerHour = 5000, RequestsPerDay = 50000 },
            ["User"] = new RoleLimits { RequestsPerMinute = 50, RequestsPerHour = 2000, RequestsPerDay = 20000 }
        };
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // User with multiple roles (Admin has priority in dictionary iteration)
        var contexts = Enumerable.Range(1, 60)
            .Select(_ => CreateHttpContext(isAuthenticated: true, userId: "user1", roles: ["Admin", "User"]))
            .ToList();

        // Act
        foreach (var context in contexts)
        {
            await middleware.InvokeAsync(context);
        }

        // Assert
        contexts.Should().AllSatisfy(c => c.Response.StatusCode.Should().Be(200)); // All OK (admin limit 100)
    }

    [Fact]
    public async Task InvokeAsync_RoleSpecificLimit_NoMatchingRole_ShouldUseDefault()
    {
        // Arrange
        _options.Authenticated.RequestsPerMinute = 5;
        _options.RoleLimits = new Dictionary<string, RoleLimits>
        {
            ["Admin"] = new RoleLimits { RequestsPerMinute = 100, RequestsPerHour = 5000, RequestsPerDay = 50000 }
        };
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // User with "User" role (not in RoleLimits)
        var context1 = CreateHttpContext(isAuthenticated: true, userId: "user1", roles: ["User"]);
        var context2 = CreateHttpContext(isAuthenticated: true, userId: "user1", roles: ["User"]);
        var context3 = CreateHttpContext(isAuthenticated: true, userId: "user1", roles: ["User"]);
        var context4 = CreateHttpContext(isAuthenticated: true, userId: "user1", roles: ["User"]);
        var context5 = CreateHttpContext(isAuthenticated: true, userId: "user1", roles: ["User"]);
        var context6 = CreateHttpContext(isAuthenticated: true, userId: "user1", roles: ["User"]);

        // Act
        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2
        await middleware.InvokeAsync(context3); // 3
        await middleware.InvokeAsync(context4); // 4
        await middleware.InvokeAsync(context5); // 5
        await middleware.InvokeAsync(context6); // 6 - exceeds default (5)

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(200);
        context4.Response.StatusCode.Should().Be(200);
        context5.Response.StatusCode.Should().Be(200);
        context6.Response.StatusCode.Should().Be(429); // Uses default limit
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task InvokeAsync_ExceedsLimit_ShouldLogWarning()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 1;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext();
        var context2 = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2 - exceeds

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rate limit exceeded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ApproachingLimit_ShouldLogInformation()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 5; // 80% threshold = 4
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // Act - make 4 requests (80% threshold)
        var contexts = Enumerable.Range(1, 4)
            .Select(_ => CreateHttpContext())
            .ToList();

        foreach (var context in contexts)
        {
            await middleware.InvokeAsync(context);
        }

        // Assert - should log information when reaching 80% threshold (request #4)
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("approaching rate limit")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ScaleToWindow Tests

    [Fact]
    public async Task InvokeAsync_CustomWindow_ShouldScaleLimitsCorrectly()
    {
        // Arrange - 30-second window
        _options.General.WindowInSeconds = 30;
        _options.Anonymous.RequestsPerMinute = 60; // 60/min = 30/30sec
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // Act - make 31 requests (should exceed scaled limit of 30)
        var contexts = Enumerable.Range(1, 31)
            .Select(_ => CreateHttpContext())
            .ToList();

        foreach (var context in contexts)
        {
            await middleware.InvokeAsync(context);
        }

        // Assert - first 30 OK, 31st rate limited
        contexts.Take(30).Should().AllSatisfy(c => c.Response.StatusCode.Should().Be(200));
        contexts.Last().Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_ScaleToWindow_HourlyLimit_ShouldBeLowerThanMinuteLimit()
    {
        // Arrange - Per-minute limit is higher than scaled hourly limit
        _options.General.WindowInSeconds = 60;
        _options.Anonymous.RequestsPerMinute = 100; // 100/min
        _options.Anonymous.RequestsPerHour = 300; // 300/hour = 5/min (lower!)
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // Act - make 6 requests (exceeds hourly scaled limit of 5/min)
        var contexts = Enumerable.Range(1, 6)
            .Select(_ => CreateHttpContext())
            .ToList();

        foreach (var context in contexts)
        {
            await middleware.InvokeAsync(context);
        }

        // Assert - first 5 OK, 6th rate limited (uses minimum of all candidates)
        contexts.Take(5).Should().AllSatisfy(c => c.Response.StatusCode.Should().Be(200));
        contexts.Last().Response.StatusCode.Should().Be(429);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task InvokeAsync_NoRemoteIpAddress_ShouldUseUnknown()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 2;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // Context with null RemoteIpAddress
        var context1 = CreateHttpContext(ip: null);
        var context2 = CreateHttpContext(ip: null);
        var context3 = CreateHttpContext(ip: null);

        // Act
        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2
        await middleware.InvokeAsync(context3); // 3 - exceeds

        // Assert
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(429); // Uses "unknown" as IP
    }

    [Fact]
    public async Task InvokeAsync_EmptyEndpointPattern_ShouldNotMatch()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 10;
        _options.EndpointLimits = new Dictionary<string, EndpointLimits>
        {
            ["empty"] = new EndpointLimits
            {
                Pattern = "", // Empty pattern
                RequestsPerMinute = 2,
                RequestsPerHour = 100,
                ApplyToAnonymous = true
            }
        };
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // Act - make 5 requests (should use default limit 10, not endpoint limit 2)
        var contexts = Enumerable.Range(1, 5)
            .Select(_ => CreateHttpContext(path: "/api/users"))
            .ToList();

        foreach (var context in contexts)
        {
            await middleware.InvokeAsync(context);
        }

        // Assert - all OK (empty pattern doesn't match)
        contexts.Should().AllSatisfy(c => c.Response.StatusCode.Should().Be(200));
    }

    [Fact]
    public async Task InvokeAsync_CaseInsensitivePathMatching_ShouldMatch()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 10;
        _options.EndpointLimits = new Dictionary<string, EndpointLimits>
        {
            ["status"] = new EndpointLimits
            {
                Pattern = "/API/STATUS", // Uppercase pattern
                RequestsPerMinute = 2,
                RequestsPerHour = 100,
                ApplyToAnonymous = true
            }
        };
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // Act - lowercase path
        var context1 = CreateHttpContext(path: "/api/status");
        var context2 = CreateHttpContext(path: "/api/status");
        var context3 = CreateHttpContext(path: "/api/status");

        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2
        await middleware.InvokeAsync(context3); // 3 - exceeds

        // Assert - case-insensitive matching
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
        context3.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_WindowClampedToMinimum_ShouldBeAtLeast1Second()
    {
        // Arrange - invalid window (0 seconds)
        _options.General.WindowInSeconds = 0; // Invalid
        _options.Anonymous.RequestsPerMinute = 60; // Should scale to 1 request/sec minimum
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);

        // Act - make 2 requests
        var context1 = CreateHttpContext();
        var context2 = CreateHttpContext();

        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2

        // Assert - both should succeed (window clamped to 1 second, limit calculated as 1)
        context1.Response.StatusCode.Should().Be(200);
        context2.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_RetryAfterHeader_ShouldReflectRemainingTTL()
    {
        // Arrange
        _options.Anonymous.RequestsPerMinute = 1;
        _options.General.WindowInSeconds = 60;
        var middleware = new RateLimitingMiddleware(_next.Object, CreateCache(), _optionsMonitor.Object, _logger.Object);
        var context1 = CreateHttpContext();
        var context2 = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context1); // 1
        await middleware.InvokeAsync(context2); // 2 - exceeds

        // Assert
        context2.Response.StatusCode.Should().Be(429);
        context2.Response.Headers.Should().ContainKey("Retry-After");
        var retryAfter = int.Parse(context2.Response.Headers["Retry-After"].ToString());
        retryAfter.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(60); // Between 0 and 60 seconds
    }

    #endregion

    #region Helper Methods

    private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

    private DefaultHttpContext CreateHttpContext(
        bool isAuthenticated = false,
        string? userId = null,
        string? userName = null,
        string[]? roles = null,
        string? ip = "127.0.0.1",
        string method = "GET",
        string path = "/api/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;

        if (ip != null)
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        }

        if (isAuthenticated)
        {
            var claims = new List<Claim>();

            if (userId != null)
            {
                claims.Add(new Claim("sub", userId));
            }

            if (userName != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, userName));
            }

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim("role", role));
                }
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            context.User = new ClaimsPrincipal(identity);
        }

        context.Response.Body = new MemoryStream();

        return context;
    }

    #endregion
}
