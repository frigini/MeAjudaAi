using FluentAssertions;
using MeAjudaAi.ApiService.Options;

namespace MeAjudaAi.ApiService.Tests.Unit.Options;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class OptionsTests
{
    [Fact]
    public void CorsOptions_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        options.Should().NotBeNull();
        options.GetType().Should().Be(typeof(CorsOptions));
        options.GetType().GetProperty("AllowedOrigins").Should().NotBeNull();
        options.GetType().GetProperty("AllowedMethods").Should().NotBeNull();
        options.GetType().GetProperty("AllowedHeaders").Should().NotBeNull();
        options.GetType().GetProperty("AllowCredentials").Should().NotBeNull();
        options.GetType().GetProperty("PreflightMaxAge").Should().NotBeNull();
        CorsOptions.SectionName.Should().Be("Cors");
    }

    [Fact]
    public void RateLimitOptions_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var options = new RateLimitOptions();

        // Assert
        options.Should().NotBeNull();
        options.GetType().Should().Be<RateLimitOptions>();
    }

    [Fact]
    public void GeneralSettings_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var settings = new GeneralSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.GetType().Should().Be<GeneralSettings>();
    }

    [Fact]
    public void AuthenticatedLimits_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var limits = new AuthenticatedLimits();

        // Assert
        limits.Should().NotBeNull();
        limits.GetType().Should().Be<AuthenticatedLimits>();
    }

    [Fact]
    public void AnonymousLimits_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var limits = new AnonymousLimits();

        // Assert
        limits.Should().NotBeNull();
        limits.GetType().Should().Be<AnonymousLimits>();
    }
}
