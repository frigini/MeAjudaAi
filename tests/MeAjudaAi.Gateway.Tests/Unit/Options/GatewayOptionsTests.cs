using FluentAssertions;
using MeAjudaAi.Gateway.Options;
using MeAjudaAi.Shared.Middleware;

namespace MeAjudaAi.Gateway.Tests.Unit.Options;

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class GatewayCorsOptionsTests
{
    [Fact]
    public void GatewayCorsOptions_DefaultValues_ShouldBeInitialized()
    {
        var options = new GatewayCorsOptions();

        options.Should().NotBeNull();
        options.AllowedOrigins.Should().BeEmpty();
        options.AllowedMethods.Should().Contain("GET");
        options.AllowedHeaders.Should().Contain("*");
        options.AllowCredentials.Should().BeTrue();
        options.MaxAgeSeconds.Should().Be(3600);
    }

    [Fact]
    public void GatewayCorsOptions_SectionName_ShouldBeCors()
    {
        GatewayCorsOptions.SectionName.Should().Be("Cors");
    }
}

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class RateLimitingOptionsTests
{
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
}

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class GeographicRestrictionOptionsTests
{
    [Fact]
    public void GeographicRestrictionOptions_DefaultValues_ShouldBeInitialized()
    {
        var options = new GeographicRestrictionOptions();

        options.Should().NotBeNull();
        options.Enabled.Should().BeFalse();
        options.FailOpen.Should().BeTrue();
        options.AllowedStates.Should().BeEmpty();
        options.AllowedCities.Should().BeEmpty();
    }

    [Fact]
    public void GeographicRestrictionOptions_SectionName_ShouldBeGeographicRestriction()
    {
        GeographicRestrictionOptions.SectionName.Should().Be("GeographicRestriction");
    }
}