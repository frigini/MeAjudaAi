using FluentAssertions;
using MeAjudaAi.Shared.Middleware.GeographicRestriction;

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
