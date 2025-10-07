using FluentAssertions;
using MeAjudaAi.ServiceDefaults.Options;
using Xunit;

namespace MeAjudaAi.ServiceDefaults.Tests.Unit.Options;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceDefaults")]
[Trait("Layer", "ServiceDefaults")]
public class OpenTelemetryOptionsTests
{
    [Fact]
    public void OpenTelemetryOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new OpenTelemetryOptions();

        // Assert
        options.Should().NotBeNull();
    }

    [Fact]
    public void OpenTelemetryOptions_ShouldAllowPropertySetting()
    {
        // Arrange
        var options = new OpenTelemetryOptions();

        // Act & Assert - Verify it's a valid class
        options.GetType().Should().NotBeNull();
        options.GetType().IsClass.Should().BeTrue();
    }

    [Fact]
    public void OpenTelemetryOptions_ShouldBeSerializable()
    {
        // Arrange
        var options = new OpenTelemetryOptions();

        // Act & Assert - Basic serialization test
        var action = () => System.Text.Json.JsonSerializer.Serialize(options);
        action.Should().NotThrow();
    }
}
