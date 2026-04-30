using FluentAssertions;
using MeAjudaAi.Gateway.Resilience;

namespace MeAjudaAi.Gateway.Tests.Unit.Resilience;

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class GatewayResilienceOptionsTests
{
    [Fact]
    public void GatewayResilienceOptions_DefaultValues_ShouldBeInitialized()
    {
        var options = new GatewayResilienceOptions();

        options.Should().NotBeNull();
        options.TimeoutSeconds.Should().Be(30);
        options.RetryCount.Should().Be(3);
    }

    [Fact]
    public void GatewayResilienceOptions_SectionName_ShouldBeGatewayResilience()
    {
        GatewayResilienceOptions.SectionName.Should().Be("GatewayResilience");
    }

    [Fact]
    public void GatewayResilienceOptions_WithCustomValues_ShouldConfigureCorrectly()
    {
        var options = new GatewayResilienceOptions
        {
            TimeoutSeconds = 60,
            RetryCount = 5
        };

        options.TimeoutSeconds.Should().Be(60);
        options.RetryCount.Should().Be(5);
    }

    [Fact]
    public void GatewayResilienceOptions_TimeoutSeconds_CanBeSetToZero()
    {
        var options = new GatewayResilienceOptions { TimeoutSeconds = 0 };

        options.TimeoutSeconds.Should().Be(0);
    }

    [Fact]
    public void GatewayResilienceOptions_RetryCount_CanBeSetToZero()
    {
        var options = new GatewayResilienceOptions { RetryCount = 0 };

        options.RetryCount.Should().Be(0);
    }
}
