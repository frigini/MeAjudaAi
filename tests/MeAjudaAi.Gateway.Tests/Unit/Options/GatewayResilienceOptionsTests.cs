using FluentAssertions;
using MeAjudaAi.Gateway.Options;

namespace MeAjudaAi.Gateway.Tests.Unit.Options;

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
        options.RetryBaseDelayMs.Should().Be(100);
        options.RetryableMethods.Should().Contain("GET");
        options.RetryableMethods.Should().Contain("HEAD");
        options.RetryableMethods.Should().Contain("OPTIONS");
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
            RetryCount = 5,
            RetryBaseDelayMs = 200,
            RetryableMethods = ["GET", "POST", "PUT"]
        };

        options.TimeoutSeconds.Should().Be(60);
        options.RetryCount.Should().Be(5);
        options.RetryBaseDelayMs.Should().Be(200);
        options.RetryableMethods.Should().HaveCount(3);
        options.RetryableMethods.Should().Contain("POST");
    }
}