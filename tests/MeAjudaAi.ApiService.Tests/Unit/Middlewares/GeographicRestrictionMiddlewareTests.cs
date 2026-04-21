using MeAjudaAi.ApiService.Middlewares;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Middlewares;

public class GeographicRestrictionMiddlewareTests
{
    private readonly Mock<ILogger<GeographicRestrictionMiddleware>> _loggerMock = new();
    private readonly Mock<IFeatureManager> _featureManagerMock = new();
    private readonly Mock<IOptionsMonitor<GeographicRestrictionOptions>> _optionsMock = new();
    private readonly Mock<IGeographicValidationService> _geoServiceMock = new();
    private readonly GeographicRestrictionOptions _options = new();

    public GeographicRestrictionMiddlewareTests()
    {
        _optionsMock.Setup(o => o.CurrentValue).Returns(_options);
        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.GeographicRestriction)).ReturnsAsync(true);
    }

    [Fact]
    public async Task InvokeAsync_WhenFeatureDisabled_ShouldCallNext()
    {
        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.GeographicRestriction)).ReturnsAsync(false);
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var sut = new GeographicRestrictionMiddleware(next, _loggerMock.Object, _optionsMock.Object, _featureManagerMock.Object);
        var context = new DefaultHttpContext();
        await sut.InvokeAsync(context);
        nextCalled.Should().BeTrue();
    }
}
