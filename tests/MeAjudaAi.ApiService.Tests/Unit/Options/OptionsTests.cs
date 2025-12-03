using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
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

    #region GeographicRestrictionOptions Tests

    [Fact]
    public void GeographicRestrictionOptions_DefaultValues_ShouldBeInitialized()
    {
        // Arrange & Act
        var options = new GeographicRestrictionOptions();

        // Assert
        options.Should().NotBeNull();
        options.AllowedStates.Should().NotBeNull();
        options.AllowedStates.Should().BeEmpty();
        options.AllowedCities.Should().NotBeNull();
        options.AllowedCities.Should().BeEmpty();
        options.BlockedMessage.Should().Be("Serviço indisponível na sua região. Disponível apenas em: {allowedRegions}");
    }

    [Fact]
    public void GeographicRestrictionOptions_AllowedStates_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new GeographicRestrictionOptions();
        var expectedStates = new List<string> { "SP", "RJ", "MG" };

        // Act
        options.AllowedStates = expectedStates;

        // Assert
        options.AllowedStates.Should().HaveCount(3);
        options.AllowedStates.Should().ContainInOrder(expectedStates);
    }

    [Fact]
    public void GeographicRestrictionOptions_AllowedCities_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new GeographicRestrictionOptions();
        var expectedCities = new List<string> { "São Paulo", "Rio de Janeiro" };

        // Act
        options.AllowedCities = expectedCities;

        // Assert
        options.AllowedCities.Should().HaveCount(2);
        options.AllowedCities.Should().ContainInOrder(expectedCities);
    }

    [Fact]
    public void GeographicRestrictionOptions_BlockedMessage_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new GeographicRestrictionOptions();
        const string customMessage = "Acesso negado para sua região";

        // Act
        options.BlockedMessage = customMessage;

        // Assert
        options.BlockedMessage.Should().Be(customMessage);
    }

    #endregion

    #region CorsOptions Validation Tests

    [Fact]
    public void CorsOptions_Validate_ShouldThrowWhenNoAllowedOrigins()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = [],
            AllowedMethods = ["GET"],
            AllowedHeaders = ["Content-Type"]
        };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*allowed origin*configured for CORS*");
    }

    [Fact]
    public void CorsOptions_Validate_ShouldThrowWhenNoAllowedMethods()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = ["http://localhost"],
            AllowedMethods = [],
            AllowedHeaders = ["Content-Type"]
        };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*allowed method*configured for CORS*");
    }

    [Fact]
    public void CorsOptions_Validate_ShouldThrowWhenNoAllowedHeaders()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = ["http://localhost"],
            AllowedMethods = ["GET"],
            AllowedHeaders = []
        };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*allowed header*configured for CORS*");
    }

    [Fact]
    public void CorsOptions_Validate_ShouldThrowWhenPreflightMaxAgeIsNegative()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = ["http://localhost"],
            AllowedMethods = ["GET"],
            AllowedHeaders = ["Content-Type"],
            PreflightMaxAge = -1
        };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*PreflightMaxAge*non-negative*");
    }

    [Fact]
    public void CorsOptions_Validate_ShouldThrowWhenOriginIsEmpty()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = ["http://localhost", ""],
            AllowedMethods = ["GET"],
            AllowedHeaders = ["Content-Type"]
        };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*allowed origins*empty values*");
    }

    [Fact]
    public void CorsOptions_Validate_ShouldThrowWhenOriginHasInvalidFormat()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = ["not-a-valid-url"],
            AllowedMethods = ["GET"],
            AllowedHeaders = ["Content-Type"]
        };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid*origin format*");
    }

    [Fact]
    public void CorsOptions_Validate_ShouldThrowWhenWildcardUsedWithCredentials()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = ["*"],
            AllowedMethods = ["GET"],
            AllowedHeaders = ["Content-Type"],
            AllowCredentials = true
        };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*wildcard*credentials*");
    }

    [Fact]
    public void CorsOptions_Validate_ShouldSucceedWithValidConfiguration()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = ["http://localhost:3000", "https://example.com"],
            AllowedMethods = ["GET", "POST"],
            AllowedHeaders = ["Content-Type", "Authorization"],
            PreflightMaxAge = 7200
        };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void CorsOptions_Validate_ShouldAllowWildcardOriginWithoutCredentials()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = ["*"],
            AllowedMethods = ["GET"],
            AllowedHeaders = ["Content-Type"],
            AllowCredentials = false
        };

        // Act & Assert
        var act = () => options.Validate();
        act.Should().NotThrow();
    }

    #endregion
}
