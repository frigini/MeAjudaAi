using FluentAssertions;
using MeAjudaAi.Shared.Middleware;
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
        options.AllowedStates.Should().BeNull();
        options.AllowedCities.Should().BeNull();
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