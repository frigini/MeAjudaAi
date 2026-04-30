using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Shared.Middleware;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;

namespace MeAjudaAi.Gateway.Tests.Unit.Middleware;

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

    [Fact]
    public void GeographicRestrictionOptions_WithAllowedStates_ShouldConfigureCorrectly()
    {
        var options = new GeographicRestrictionOptions
        {
            Enabled = true,
            AllowedStates = ["SP", "RJ", "MG"],
            AllowedCities = ["São Paulo", "Rio de Janeiro"],
            FailOpen = false
        };

        options.Enabled.Should().BeTrue();
        options.AllowedStates.Should().HaveCount(3);
        options.AllowedCities.Should().HaveCount(2);
        options.FailOpen.Should().BeFalse();
    }
}

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class GeographicRestrictionErrorResponseTests
{
    [Fact]
    public void GeographicRestrictionErrorResponse_ShouldCreateCorrectly()
    {
        var response = new GeographicRestrictionErrorResponse(
            "Access denied",
            UserLocation.Create("São Paulo", "SP"),
            [AllowedCity.Create("São Paulo", "SP")],
            ["SP", "RJ"]
        );

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
        var allowedCity = AllowedCity.Create("São Paulo", "SP");

        allowedCity.Name.Should().Be("São Paulo");
        allowedCity.State.Should().Be("SP");
    }

    [Fact]
    public void UserLocation_Create_ShouldSetPropertiesCorrectly()
    {
        var userLocation = UserLocation.Create("São Paulo", "SP");

        userLocation.City.Should().Be("São Paulo");
        userLocation.State.Should().Be("SP");
    }

    [Fact]
    public void UserLocation_Create_WithNullValues_ShouldWork()
    {
        var userLocation = UserLocation.Create(null, null);

        userLocation.City.Should().BeNull();
        userLocation.State.Should().BeNull();
    }
}

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
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
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(false);

        var context = new DefaultHttpContext();
        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_BlockedState_Returns451()
    {
        var middleware = CreateMiddleware();

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "Salvador|BA";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_AllowedState_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "São Paulo|SP";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_FailOpenTrue_NoHeader_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_HealthPath_Bypasses()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_MalformedHeader_IsAlwaysBlocked()
    {
        var middleware = CreateMiddleware(configure: opts => opts.FailOpen = true);

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "|";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_MalformedHeader_FailOpenFalse_Returns451()
    {
        var middleware = CreateMiddleware(configure: opts => opts.FailOpen = false);

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "|";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_FailOpenFalse_NoHeader_Returns451()
    {
        var middleware = CreateMiddleware(configure: opts => opts.FailOpen = false);

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_OptionsDisabled_FeatureEnabled_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            configure: opts => { opts.Enabled = false; opts.FailOpen = false; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Location"] = "Blockedville|BA";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SwaggerPath_Bypasses()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_FrameworkPath_Bypasses()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Path = "/_framework/blazor.webassembly.js";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_XUserCityHeader_AllowedCity_CallsNext()
    {
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

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_XUserCityHeader_BlockedCity_Returns451()
    {
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

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_AllowedCityWithStateFormat_MatchingCity_CallsNext()
    {
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

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AllowedCityWithStateFormat_WrongState_Returns451()
    {
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

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_CityOnlyConfig_MatchingCity_CallsNext()
    {
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

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_IbgeServiceException_FallsBackToSimpleValidation_AndBlocks()
    {
        var middleware = CreateMiddleware(configure: opts =>
        {
            opts.AllowedStates = ["SP"];
            opts.AllowedCities = [];
        });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        // Simulate IBGE service that throws an exception
        var ibgeServiceMock = new Mock<MeAjudaAi.Shared.Geolocation.IGeographicValidationService>();
        ibgeServiceMock
            .Setup(x => x.ValidateCityAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("IBGE unavailable"));

        var context = new DefaultHttpContext();
        // City "Salvador" / state "BA" is NOT in AllowedStates (only SP allowed)
        // simpleValidation = false → IBGE is invoked → throws → fallback to simpleValidation (false) → blocked
        context.Request.Headers["X-User-Location"] = "Salvador|BA";

        await middleware.InvokeAsync(context, ibgeServiceMock.Object);

        context.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task InvokeAsync_BlockedResponse_ContainsExpectedJsonFields()
    {
        var middleware = CreateMiddleware(configure: opts =>
        {
            opts.AllowedStates = ["SP"];
            opts.AllowedCities = ["São Paulo|SP"];
            opts.BlockedMessage = "Acesso bloqueado. Regiões: {allowedRegions}.";
        });

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(true);

        var responseBody = new System.IO.MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = responseBody;
        context.Request.Headers["X-User-Location"] = "Salvador|BA";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(451);
        context.Response.ContentType.Should().Be("application/json");
        responseBody.Seek(0, System.IO.SeekOrigin.Begin);
        var json = await new System.IO.StreamReader(responseBody).ReadToEndAsync();
        json.Should().Contain("message");
        json.Should().Contain("userLocation");
    }

    [Fact]
    public async Task InvokeAsync_NoAllowedCitiesOrStates_AllowsEveryone()
    {
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

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}