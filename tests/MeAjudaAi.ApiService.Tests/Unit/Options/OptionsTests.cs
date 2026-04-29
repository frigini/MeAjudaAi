using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.ApiService.Options.RateLimit;

namespace MeAjudaAi.ApiService.Tests.Unit.Options;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class OptionsTests
{
    [Fact]
    public void RateLimitOptions_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var options = new RateLimitOptions();

        // Assert
        options.Should().NotBeNull();
        options.Should().BeOfType<RateLimitOptions>();
    }

    [Fact]
    public void GeneralSettings_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var settings = new GeneralSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.Should().BeOfType<GeneralSettings>();
    }

    [Fact]
    public void AuthenticatedLimits_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var limits = new AuthenticatedLimits();

        // Assert
        limits.Should().NotBeNull();
        limits.Should().BeOfType<AuthenticatedLimits>();
    }

    [Fact]
    public void AnonymousLimits_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var limits = new AnonymousLimits();

        // Assert
        limits.Should().NotBeNull();
        limits.Should().BeOfType<AnonymousLimits>();
    }

    #region SecurityOptions Tests

    [Fact]
    public void SecurityOptions_DefaultValues_ShouldBeInitialized()
    {
        // Arrange & Act
        var options = new SecurityOptions();

        // Assert
        options.Should().NotBeNull();
        options.EnforceHttps.Should().BeFalse();
        options.EnableStrictTransportSecurity.Should().BeFalse();
        options.AllowedHosts.Should().NotBeNull();
        options.AllowedHosts.Should().BeEmpty();
    }

    [Fact]
    public void SecurityOptions_EnforceHttps_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new SecurityOptions();

        // Act
        options.EnforceHttps = true;

        // Assert
        options.EnforceHttps.Should().BeTrue();
    }

    [Fact]
    public void SecurityOptions_EnableStrictTransportSecurity_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new SecurityOptions();

        // Act
        options.EnableStrictTransportSecurity = true;

        // Assert
        options.EnableStrictTransportSecurity.Should().BeTrue();
    }

    [Fact]
    public void SecurityOptions_AllowedHosts_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new SecurityOptions();
        var expectedHosts = new List<string> { "localhost", "example.com", "*.meajudaai.com" };

        // Act
        options.AllowedHosts = expectedHosts;

        // Assert
        options.AllowedHosts.Should().NotBeNull();
        options.AllowedHosts.Should().HaveCount(3);
        options.AllowedHosts.Should().ContainInOrder(expectedHosts);
    }

    #endregion

    #region EndpointLimits Tests

    [Fact]
    public void EndpointLimits_DefaultValues_ShouldBeInitialized()
    {
        // Arrange & Act
        var limits = new EndpointLimits();

        // Assert
        limits.Should().NotBeNull();
        limits.Pattern.Should().Be(string.Empty);
        limits.RequestsPerMinute.Should().Be(60);
        limits.RequestsPerHour.Should().Be(1000);
        limits.ApplyToAuthenticated.Should().BeTrue();
        limits.ApplyToAnonymous.Should().BeTrue();
    }

    [Fact]
    public void EndpointLimits_Pattern_CanBeSetAndRetrieved()
    {
        // Arrange
        var limits = new EndpointLimits();
        const string expectedPattern = "/api/v1/users/*";

        // Act
        limits.Pattern = expectedPattern;

        // Assert
        limits.Pattern.Should().Be(expectedPattern);
    }

    [Fact]
    public void EndpointLimits_RequestsPerMinute_CanBeSetAndRetrieved()
    {
        // Arrange
        var limits = new EndpointLimits();

        // Act
        limits.RequestsPerMinute = 120;

        // Assert
        limits.RequestsPerMinute.Should().Be(120);
    }

    [Fact]
    public void EndpointLimits_RequestsPerHour_CanBeSetAndRetrieved()
    {
        // Arrange
        var limits = new EndpointLimits();

        // Act
        limits.RequestsPerHour = 2000;

        // Assert
        limits.RequestsPerHour.Should().Be(2000);
    }

    [Fact]
    public void EndpointLimits_ApplyToAuthenticated_CanBeSetAndRetrieved()
    {
        // Arrange
        var limits = new EndpointLimits();

        // Act
        limits.ApplyToAuthenticated = false;

        // Assert
        limits.ApplyToAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void EndpointLimits_ApplyToAnonymous_CanBeSetAndRetrieved()
    {
        // Arrange
        var limits = new EndpointLimits();

        // Act
        limits.ApplyToAnonymous = false;

        // Assert
        limits.ApplyToAnonymous.Should().BeFalse();
    }

    #endregion

    #region RoleLimits Tests

    [Fact]
    public void RoleLimits_DefaultValues_ShouldBeInitialized()
    {
        // Arrange & Act
        var limits = new RoleLimits();

        // Assert
        limits.Should().NotBeNull();
        limits.RequestsPerMinute.Should().Be(200);
        limits.RequestsPerHour.Should().Be(5000);
        limits.RequestsPerDay.Should().Be(20000);
    }

    [Fact]
    public void RoleLimits_RequestsPerMinute_CanBeSetAndRetrieved()
    {
        // Arrange
        var limits = new RoleLimits();

        // Act
        limits.RequestsPerMinute = 300;

        // Assert
        limits.RequestsPerMinute.Should().Be(300);
    }

    [Fact]
    public void RoleLimits_RequestsPerHour_CanBeSetAndRetrieved()
    {
        // Arrange
        var limits = new RoleLimits();

        // Act
        limits.RequestsPerHour = 10000;

        // Assert
        limits.RequestsPerHour.Should().Be(10000);
    }

    [Fact]
    public void RoleLimits_RequestsPerDay_CanBeSetAndRetrieved()
    {
        // Arrange
        var limits = new RoleLimits();

        // Act
        limits.RequestsPerDay = 50000;

        // Assert
        limits.RequestsPerDay.Should().Be(50000);
    }

    #endregion
}
