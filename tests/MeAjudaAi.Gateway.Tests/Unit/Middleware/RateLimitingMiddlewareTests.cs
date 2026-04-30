using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Shared.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MeAjudaAi.Gateway.Tests.Unit.Middleware;

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class RateLimitingMiddlewareTests
{
    private readonly Mock<ILogger<RateLimitingMiddleware>> _loggerMock;
    private readonly Mock<IOptionsMonitor<RateLimitingOptions>> _optionsMock;
    private readonly IMemoryCache _cache;
    private readonly RateLimitingOptions _options;

    public RateLimitingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
        _optionsMock = new Mock<IOptionsMonitor<RateLimitingOptions>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _options = new RateLimitingOptions
        {
            General = new GeneralSettings
            {
                Enabled = true,
                WindowInSeconds = 60,
                EnableIpWhitelist = false,
                WhitelistedIps = [],
                ErrorMessage = "Rate limit exceeded"
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
            }
        };
        _optionsMock.Setup(x => x.CurrentValue).Returns(_options);
    }

    [Fact]
    public void RateLimitingOptions_DefaultValues_ShouldBeInitialized()
    {
        var options = new RateLimitingOptions();

        options.Should().NotBeNull();
        options.General.Enabled.Should().BeTrue();
        options.General.WindowInSeconds.Should().Be(60);
        options.General.EnableIpWhitelist.Should().BeFalse();
        options.General.WhitelistedIps.Should().BeEmpty();
        options.Anonymous.RequestsPerMinute.Should().Be(30);
        options.Authenticated.RequestsPerMinute.Should().Be(120);
    }

    [Fact]
    public void RateLimitingOptions_SectionName_ShouldBeRateLimiting()
    {
        RateLimitingOptions.SectionName.Should().Be("RateLimiting");
    }

    [Fact]
    public void GeneralSettings_DefaultValues_ShouldBeInitialized()
    {
        var settings = new GeneralSettings();

        settings.Enabled.Should().BeTrue();
        settings.WindowInSeconds.Should().Be(60);
        settings.EnableIpWhitelist.Should().BeFalse();
        settings.WhitelistedIps.Should().BeEmpty();
        settings.ErrorMessage.Should().Be("Limite de requisições excedido. Tente novamente mais tarde.");
    }

    [Fact]
    public void AnonymousLimits_DefaultValues_ShouldBeInitialized()
    {
        var limits = new AnonymousLimits();

        limits.RequestsPerMinute.Should().Be(30);
        limits.RequestsPerHour.Should().Be(300);
        limits.RequestsPerDay.Should().Be(1000);
    }

    [Fact]
    public void AuthenticatedLimits_DefaultValues_ShouldBeInitialized()
    {
        var limits = new AuthenticatedLimits();

        limits.RequestsPerMinute.Should().Be(120);
        limits.RequestsPerHour.Should().Be(2000);
        limits.RequestsPerDay.Should().Be(10000);
    }

    [Fact]
    public void RateLimitCounter_IncrementAndGet_ShouldReturnSequentialValues()
    {
        var counter = new RateLimitCounter();

        counter.IncrementAndGet().Should().Be(1);
        counter.IncrementAndGet().Should().Be(2);
        counter.IncrementAndGet().Should().Be(3);
        counter.Value.Should().Be(3);
    }
}

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class RateLimitingMiddlewareBehaviorTests
{
    private Mock<ILogger<RateLimitingMiddleware>> _loggerMock = null!;
    private Mock<IOptionsMonitor<RateLimitingOptions>> _optionsMock = null!;
    private IMemoryCache _cache = null!;

    private RateLimitingMiddleware CreateMiddleware(
        RequestDelegate? next = null,
        Action<RateLimitingOptions>? configureOptions = null)
    {
        _loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
        _optionsMock = new Mock<IOptionsMonitor<RateLimitingOptions>>();
        _cache = new MemoryCache(new MemoryCacheOptions());

        var options = new RateLimitingOptions
        {
            General = new GeneralSettings
            {
                Enabled = true,
                WindowInSeconds = 60,
                EnableIpWhitelist = false,
                WhitelistedIps = [],
                ErrorMessage = "Rate limit exceeded"
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
            }
        };
        configureOptions?.Invoke(options);
        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        next ??= _ => Task.CompletedTask;
        return new RateLimitingMiddleware(
            next,
            _loggerMock.Object,
            _optionsMock.Object,
            _cache);
    }

    [Fact]
    public async Task InvokeAsync_Disabled_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, opts =>
        {
            opts.General.Enabled = false;
        });

        var context = new DefaultHttpContext();
        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhitelistedIp_Bypasses()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, opts =>
        {
            opts.General.EnableIpWhitelist = true;
            opts.General.WhitelistedIps = ["127.0.0.1"];
        });

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_LimitExceeded_Returns429WithRetryAfter()
    {
        var middleware = CreateMiddleware();

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(429);
        context.Response.Headers["Retry-After"].Should().NotBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_UnderLimit_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_UsesHigherLimit()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(next: _ => { nextCalled = true; return Task.CompletedTask; }, configureOptions: opts =>
        {
            opts.Anonymous.RequestsPerMinute = 2;
            opts.Authenticated.RequestsPerMinute = 100;
        });

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
        var claims = new List<System.Security.Claims.Claim>
        {
            new("sub", "user123")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);

        for (int i = 0; i < 3; i++)
        {
            await middleware.InvokeAsync(context);
        }

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AnonymousUser_UsesLowerLimit()
    {
        var middleware = CreateMiddleware(configureOptions: opts =>
        {
            opts.Anonymous.RequestsPerMinute = 2;
            opts.Authenticated.RequestsPerMinute = 100;
        });

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_LimitExceeded_SetsCorrectStatusCodeAndHeaders()
    {
        var loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
        var optionsMock = new Mock<IOptionsMonitor<RateLimitingOptions>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        
        var options = new RateLimitingOptions
        {
            General = new GeneralSettings
            {
                Enabled = true,
                WindowInSeconds = 60,
                ErrorMessage = "Custom rate limit message"
            },
            Anonymous = new AnonymousLimits
            {
                RequestsPerMinute = 30,
                RequestsPerHour = 300,
                RequestsPerDay = 1000
            }
        };
        optionsMock.Setup(x => x.CurrentValue).Returns(options);

        var middleware = new RateLimitingMiddleware(
            _ => Task.CompletedTask,
            loggerMock.Object,
            optionsMock.Object,
            cache);

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.1");
        context.Request.Method = "GET";

        for (int i = 0; i < 35; i++)
        {
            await middleware.InvokeAsync(context);
        }

        context.Response.StatusCode.Should().Be(429);
        context.Response.ContentType.Should().Contain("application/json");
        context.Response.Headers["Retry-After"].Should().NotBeEmpty();
    }
}