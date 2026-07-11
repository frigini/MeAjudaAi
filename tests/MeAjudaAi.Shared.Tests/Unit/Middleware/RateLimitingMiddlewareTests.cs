using MeAjudaAi.Shared.Middleware;
using MeAjudaAi.Shared.Middleware.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Shared.Tests.Unit.Middleware;

[Trait("Category", "Unit")]
[Trait("Layer", "Shared")]
public class RateLimitingOptionsTests
{
    [Fact]
    public void RateLimitingOptions_DefaultValues_ShouldBeInitialized()
    {
        // Arrange
        var options = new RateLimitingOptions();

        // Assert
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
        // Arrange & Act
        var sectionName = RateLimitingOptions.SectionName;

        // Assert
        sectionName.Should().Be("RateLimiting");
    }

    [Fact]
    public void GeneralSettings_DefaultValues_ShouldBeInitialized()
    {
        // Arrange
        var settings = new GeneralSettings();

        // Assert
        settings.Enabled.Should().BeTrue();
        settings.WindowInSeconds.Should().Be(60);
        settings.EnableIpWhitelist.Should().BeFalse();
        settings.WhitelistedIps.Should().BeEmpty();
        settings.ErrorMessage.Should().Be("Limite de requisições excedido. Tente novamente mais tarde.");
    }

    [Fact]
    public void AnonymousLimits_DefaultValues_ShouldBeInitialized()
    {
        // Arrange
        var limits = new AnonymousLimits();

        // Assert
        limits.RequestsPerMinute.Should().Be(30);
        limits.RequestsPerHour.Should().Be(300);
        limits.RequestsPerDay.Should().Be(1000);
    }

    [Fact]
    public void AuthenticatedLimits_DefaultValues_ShouldBeInitialized()
    {
        // Arrange
        var limits = new AuthenticatedLimits();

        // Assert
        limits.RequestsPerMinute.Should().Be(120);
        limits.RequestsPerHour.Should().Be(2000);
        limits.RequestsPerDay.Should().Be(10000);
    }

    [Fact]
    public void RateLimitCounter_IncrementAndGet_ShouldReturnSequentialValues()
    {
        // Arrange
        var counter = new RateLimitCounter();

        // Act & Assert
        counter.IncrementAndGet().Should().Be(1);
        counter.IncrementAndGet().Should().Be(2);
        counter.IncrementAndGet().Should().Be(3);
        counter.Value.Should().Be(3);
    }
}

[Trait("Category", "Unit")]
[Trait("Layer", "Shared")]
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
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, opts =>
        {
            opts.General.Enabled = false;
        });
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhitelistedIp_Bypasses()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, opts =>
        {
            opts.General.EnableIpWhitelist = true;
            opts.General.WhitelistedIps = ["127.0.0.1"];
        });
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_LimitExceeded_Returns429WithRetryAfter()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        // Act
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
        context.Response.Headers["Retry-After"].Should().NotBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_UnderLimit_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_UsesHigherLimit()
    {
        // Arrange
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

        // Act
        for (int i = 0; i < 3; i++)
        {
            await middleware.InvokeAsync(context);
        }

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AnonymousUser_UsesLowerLimit()
    {
        // Arrange
        var middleware = CreateMiddleware(configureOptions: opts =>
        {
            opts.Anonymous.RequestsPerMinute = 2;
            opts.Authenticated.RequestsPerMinute = 100;
        });
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        // Act
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_LimitExceeded_ResponseBodyContainsJsonFields()
    {
        // Arrange
        var middleware = CreateMiddleware(configureOptions: opts =>
        {
            opts.Anonymous.RequestsPerMinute = 1;
            opts.Anonymous.RequestsPerHour = 100;
            opts.Anonymous.RequestsPerDay = 10000;
            opts.General.ErrorMessage = "Too many requests";
        });
        var responseBody = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = responseBody;
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.1");

        // Act
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
        responseBody.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(responseBody).ReadToEndAsync();
        json.Should().Contain("RateLimitExceeded");
        json.Should().Contain("retryAfterSeconds");
    }

    [Fact]
    public async Task InvokeAsync_NullRemoteIpAddress_UsesUnknown()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; },
            configureOptions: opts =>
            {
                opts.Anonymous.RequestsPerMinute = 100;
            });
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ZeroWindowSeconds_UsesMinimumOfOne()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; },
            configureOptions: opts =>
            {
                opts.General.WindowInSeconds = 0;
                opts.Anonymous.RequestsPerMinute = 100;
            });
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.2");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhitelistEnabled_NonWhitelistedIp_IsRateLimited()
    {
        // Arrange
        var middleware = CreateMiddleware(configureOptions: opts =>
        {
            opts.General.EnableIpWhitelist = true;
            opts.General.WhitelistedIps = ["127.0.0.1"];
            opts.Anonymous.RequestsPerMinute = 1;
        });
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.5.5");

        // Act
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_LimitExceeded_SetsCorrectStatusCodeAndHeaders()
    {
        // Arrange
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

        // Act
        for (int i = 0; i < 35; i++)
        {
            await middleware.InvokeAsync(context);
        }

        // Assert
        context.Response.StatusCode.Should().Be(429);
        context.Response.ContentType.Should().Contain("application/json");
        context.Response.Headers.RetryAfter.Should().NotBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_LimitExceeded_WritesJsonBody()
    {
        // Arrange
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

        // Act
        for (int i = 0; i < 35; i++)
            await middleware.InvokeAsync(context);

        // Assert
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
