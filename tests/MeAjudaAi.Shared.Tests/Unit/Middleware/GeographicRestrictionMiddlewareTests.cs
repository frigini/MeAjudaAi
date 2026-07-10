using System.Text;
using FluentAssertions;
using MeAjudaAi.Shared.Middleware;
using MeAjudaAi.Shared.Middleware.GeographicRestriction;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Middleware;

[Trait("Category", "Unit")]
[Trait("Layer", "Shared")]
public class GeographicRestrictionOptionsTests
{
    [Fact]
    public void GeographicRestrictionOptions_DefaultValues_ShouldBeInitialized()
    {
        // Arrange
        var options = new GeographicRestrictionOptions();

        // Act & Assert
        options.Should().NotBeNull();
        options.Enabled.Should().BeFalse();
        options.FailOpen.Should().BeTrue();
        options.AllowedStates.Should().BeEmpty();
        options.AllowedCities.Should().BeEmpty();
    }

    [Fact]
    public void GeographicRestrictionOptions_SectionName_ShouldBeGeographicRestriction()
    {
        // Arrange & Act
        var sectionName = GeographicRestrictionOptions.SectionName;

        // Assert
        sectionName.Should().Be("GeographicRestriction");
    }

    [Fact]
    public void GeographicRestrictionOptions_WithAllowedStates_ShouldConfigureCorrectly()
    {
        // Arrange & Act
        var options = new GeographicRestrictionOptions
        {
            Enabled = true,
            AllowedStates = ["SP", "RJ", "MG"],
            AllowedCities = ["São Paulo", "Rio de Janeiro"],
            FailOpen = false
        };

        // Assert
        options.Enabled.Should().BeTrue();
        options.AllowedStates.Should().HaveCount(3);
        options.AllowedCities.Should().HaveCount(2);
        options.FailOpen.Should().BeFalse();
    }
}

[Trait("Category", "Unit")]
[Trait("Layer", "Shared")]
public class GeographicRestrictionErrorResponseTests
{
    [Fact]
    public void GeographicRestrictionErrorResponse_ShouldCreateCorrectly()
    {
        // Arrange
        var response = new GeographicRestrictionErrorResponse(
            "Access denied",
            UserLocation.Create("São Paulo", "SP"),
            [AllowedCity.Create("São Paulo", "SP")],
            ["SP", "RJ"]
        );

        // Assert
        response.message.Should().Be("Access denied");
        response.userLocation.Should().NotBeNull();
        response.userLocation.City.Should().Be("São Paulo");
        response.userLocation.State.Should().Be("SP");
        response.allowedCities.Should().HaveCount(1);
        response.allowedStates.Should().HaveCount(2);
    }

    [Fact]
    public void AllowedCity_Create_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var allowedCity = AllowedCity.Create("São Paulo", "SP");

        // Assert
        allowedCity.Name.Should().Be("São Paulo");
        allowedCity.State.Should().Be("SP");
    }

    [Fact]
    public void UserLocation_Create_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var userLocation = UserLocation.Create("São Paulo", "SP");

        // Assert
        userLocation.City.Should().Be("São Paulo");
        userLocation.State.Should().Be("SP");
    }

    [Fact]
    public void UserLocation_Create_WithNullValues_ShouldWork()
    {
        // Arrange & Act
        var userLocation = UserLocation.Create(null, null);

        // Assert
        userLocation.City.Should().BeNull();
        userLocation.State.Should().BeNull();
    }
}

[Trait("Category", "Unit")]
[Trait("Layer", "Shared")]
public class GeographicRestrictionMiddlewareBehaviorTests
{
    private readonly Mock<ILogger<GeographicRestrictionMiddleware>> _loggerMock;
    private readonly Mock<IOptionsMonitor<GeographicRestrictionOptions>> _optionsMock;
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly GeographicRestrictionOptions _options;

    public GeographicRestrictionMiddlewareBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<GeographicRestrictionMiddleware>>();
        _optionsMock = new Mock<IOptionsMonitor<GeographicRestrictionOptions>>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _options = new GeographicRestrictionOptions
        {
            Enabled = true,
            FailOpen = true,
            AllowedStates = ["SP", "RJ"],
            AllowedCities = []
        };
        _optionsMock.Setup(x => x.CurrentValue).Returns(_options);
    }

    private GeographicRestrictionMiddleware CreateMiddleware(
        RequestDelegate? next = null,
        Action<GeographicRestrictionOptions>? configure = null)
    {
        var options = new GeographicRestrictionOptions
        {
            Enabled = true,
            FailOpen = true,
            AllowedStates = ["SP", "RJ"],
            AllowedCities = []
        };
        configure?.Invoke(options);

        _optionsMock.Setup(x => x.CurrentValue).Returns(options);

        next ??= _ => Task.CompletedTask;
        return new GeographicRestrictionMiddleware(
            next,
            _loggerMock.Object,
            _optionsMock.Object,
            _featureManagerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_FeatureFlagDisabled_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(false);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_BlockedState_Returns451()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "Salvador|BA";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_AllowedState_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "São Paulo|SP";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_FailOpenTrue_NoHeader_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_HealthPath_Bypasses()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_MalformedHeader_IsAlwaysBlocked()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, configure: opts => opts.FailOpen = true);

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "|";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_MalformedHeader_FailOpenFalse_Returns451()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, configure: opts => opts.FailOpen = false);

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "|";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_FailOpenFalse_NoHeader_Returns451()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, configure: opts => opts.FailOpen = false);

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_OptionsDisabled_FeatureEnabled_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            configure: opts => { opts.Enabled = false; opts.FailOpen = false; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "Blockedville|BA";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SwaggerPath_Bypasses()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_FrameworkPath_Bypasses()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Path = "/_framework/blazor.webassembly.js";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_XUserCityHeader_AllowedCity_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            configure: opts =>
            {
                opts.AllowedStates = ["SP"];
                opts.AllowedCities = [];
            });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-City"] = "São Paulo";
        context.Request.Headers["X-User-State"] = "SP";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_XUserCityHeader_BlockedCity_Returns451()
    {
        // Arrange
        var middleware = CreateMiddleware(configure: opts =>
        {
            opts.AllowedStates = ["SP"];
            opts.AllowedCities = [];
        });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-City"] = "Salvador";
        context.Request.Headers["X-User-State"] = "BA";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_AllowedCityWithStateFormat_MatchingCity_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            configure: opts =>
            {
                opts.AllowedStates = [];
                opts.AllowedCities = ["São Paulo|SP", "Rio de Janeiro|RJ"];
            });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "São Paulo|SP";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AllowedCityWithStateFormat_WrongState_Returns451()
    {
        // Arrange
        var middleware = CreateMiddleware(configure: opts =>
        {
            opts.AllowedStates = [];
            opts.AllowedCities = ["São Paulo|SP"];
        });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "São Paulo|RJ";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_CityOnlyConfig_MatchingCity_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            configure: opts =>
            {
                opts.AllowedStates = [];
                opts.AllowedCities = ["São Paulo"];
            });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "São Paulo|SP";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_IbgeServiceException_FallsBackToSimpleValidation_AndBlocks()
    {
        // Arrange
        var middleware = CreateMiddleware(configure: opts =>
        {
            opts.AllowedStates = ["SP"];
            opts.AllowedCities = [];
        });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var ibgeServiceMock = new Mock<MeAjudaAi.Shared.Geolocation.IGeographicValidationService>();
        ibgeServiceMock
            .Setup(x => x.ValidateCityAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("IBGE unavailable"));

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "Salvador|BA";

        // Act
        await middleware.InvokeAsync(context, ibgeServiceMock.Object);

        // Assert
        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_BlockedResponse_ContainsExpectedJsonFields()
    {
        // Arrange
        var middleware = CreateMiddleware(configure: opts =>
        {
            opts.AllowedStates = ["SP"];
            opts.AllowedCities = ["São Paulo|SP"];
            opts.BlockedMessage = "Acesso bloqueado. Regiões: {allowedRegions}.";
        });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var responseBody = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = responseBody;
        context.Request.Headers["X-User-Location"] = "Salvador|BA";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(451);
        context.Response.ContentType.Should().Be("application/json");
        responseBody.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(responseBody).ReadToEndAsync();
        json.Should().Contain("message");
        json.Should().Contain("userLocation");
    }

    [Fact]
    public async Task InvokeAsync_NoAllowedCitiesOrStates_AllowsEveryone()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            configure: opts =>
            {
                opts.AllowedStates = [];
                opts.AllowedCities = [];
            });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "Anywhere|XX";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_BlockedCity_Returns451()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, configure: opts =>
        {
            opts.AllowedCities = ["Muriaé"];
            opts.AllowedStates = [];
        });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "São Paulo|SP";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_BlockedCityWithAllowedStates_Returns451()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, configure: opts =>
        {
            opts.AllowedCities = [];
            opts.AllowedStates = ["MG", "RJ"];
        });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "São Paulo|SP";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_BlockedCityWithPipeFormat_Returns451WithProperResponse()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, configure: opts =>
        {
            opts.AllowedCities = ["Muriaé"];
            opts.AllowedStates = ["MG"];
            opts.DefaultBlockedMessage = "Default message: {allowedRegions}";
        });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "São Paulo|SP";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(451);
        context.Response.ContentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task InvokeAsync_AllowedCity_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, configure: opts =>
        {
            opts.Enabled = true;
            opts.FailOpen = false;
            opts.AllowedCities = ["Muriaé|MG"];
            opts.AllowedStates = [];
        });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "Muriaé|MG";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }
}
