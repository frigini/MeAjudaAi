using MeAjudaAi.Shared.Serialization;
using FluentAssertions;
using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeAjudaAi.Shared.Tests.Unit.Serialization;

[Trait("Category", "Unit")]
public class SerializationDefaultsTests
{
    [Fact]
    public void DefaultOptions_ShouldHaveCorrectSettings()
    {
        // Arrange & Act
        var options = SerializationDefaults.Default;

        // Assert
        options.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
        options.PropertyNameCaseInsensitive.Should().BeTrue();
        options.DefaultIgnoreCondition.Should().Be(JsonIgnoreCondition.WhenWritingNull);
        options.Converters.Should().NotBeEmpty();
    }

    [Fact]
    public void ApiOptions_ShouldNotBeIndented()
    {
        // Arrange & Act
        var options = SerializationDefaults.Api;

        // Assert
        options.WriteIndented.Should().BeFalse();
    }

    [Fact]
    public void LoggingOptions_ShouldBeIndented()
    {
        // Arrange & Act
        var options = SerializationDefaults.Logging;

        // Assert
        options.WriteIndented.Should().BeTrue();
        options.DefaultIgnoreCondition.Should().Be(JsonIgnoreCondition.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HealthChecksOptions_ShouldHaveCorrectNamingPolicy(bool isDevelopment)
    {
        // Arrange & Act
        var options = SerializationDefaults.HealthChecks(isDevelopment);

        // Assert
        options.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
        options.WriteIndented.Should().Be(isDevelopment);
    }
}
