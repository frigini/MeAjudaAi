using FluentAssertions;
using MeAjudaAi.Shared.Middleware;
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