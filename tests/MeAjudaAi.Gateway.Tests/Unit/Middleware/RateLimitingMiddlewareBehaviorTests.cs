using FluentAssertions;
using MeAjudaAi.Shared.Middleware;
using MeAjudaAi.Shared.Middleware.GeographicRestriction;
using MeAjudaAi.Shared.Middleware.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace MeAjudaAi.Gateway.Tests.Unit.Middleware;

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class RateLimitingMiddlewareBehaviorTests : IDisposable
{
    private Mock<ILogger<RateLimitingMiddleware>> _loggerMock = null!;
    private Mock<IOptionsMonitor<RateLimitingOptions>> _optionsMock = null!;
    private IMemoryCache _cache = null!;
    private bool _disposed;

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
    public async Task InvokeAsync_LimitExceeded_ResponseBodyContainsJsonFields()
    {
        var middleware = CreateMiddleware(configureOptions: opts =>
        {
            opts.Anonymous.RequestsPerMinute = 1;
            opts.Anonymous.RequestsPerHour = 100;
            opts.Anonymous.RequestsPerDay = 10000;
            opts.General.ErrorMessage = "Too many requests";
        });

        var responseBody = new System.IO.MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = responseBody;
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.1");

        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(429);

        responseBody.Seek(0, System.IO.SeekOrigin.Begin);
        var json = await new System.IO.StreamReader(responseBody).ReadToEndAsync();
        json.Should().Contain("RateLimitExceeded");
        json.Should().Contain("retryAfterSeconds");
    }

    [Fact]
    public async Task InvokeAsync_NullRemoteIpAddress_UsesUnknown()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; },
            configureOptions: opts =>
            {
                opts.Anonymous.RequestsPerMinute = 100;
            });

        var context = new DefaultHttpContext();
        // RemoteIpAddress é null by default em DefaultHttpContext

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ZeroWindowSeconds_UsesMinimumOfOne()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; },
            configureOptions: opts =>
            {
                opts.General.WindowInSeconds = 0; // Deve ser fixado em 1
                opts.Anonymous.RequestsPerMinute = 100;
            });

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.2");

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhitelistEnabled_NonWhitelistedIp_IsRateLimited()
    {
        var middleware = CreateMiddleware(configureOptions: opts =>
        {
            opts.General.EnableIpWhitelist = true;
            opts.General.WhitelistedIps = ["127.0.0.1"];
            opts.Anonymous.RequestsPerMinute = 1;
        });

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.5.5");

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
        context.Response.Headers.RetryAfter.Should().NotBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_LimitExceeded_WritesJsonBody()
    {
        var loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
        var optionsMock = new Mock<IOptionsMonitor<RateLimitingOptions>>();
        var cache = new MemoryCache(new MemoryCacheOptions());

        var options = new RateLimitingOptions
        {
            General = new GeneralSettings
            {
                Enabled = true,
                WindowInSeconds = 60
            },
            Anonymous = new AnonymousLimits
            {
                RequestsPerMinute = 30
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
        context.Response.Body = new MemoryStream();

        for (int i = 0; i < 35; i++)
            await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(429);
        context.Response.ContentType.Should().Contain("application/json");

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        json.Should().Contain("\"error\"");
        json.Should().Contain("\"retryAfterSeconds\"");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            try
            {
                if (_cache is IDisposable disposableCache)
                {
                    disposableCache.Dispose();
                }
            }
            catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException)
            {
                // Ignorar exceções de descarte conhecidas/esperadas para evitar desmontagem ruidosa dos testes
            }
            finally
            {
                _cache = null!;
            }
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
