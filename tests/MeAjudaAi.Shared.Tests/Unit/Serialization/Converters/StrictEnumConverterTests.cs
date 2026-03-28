using System.Text.Json;
using System.Text.Json.Serialization;
using MeAjudaAi.Shared.Serialization.Converters;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Serialization.Converters;

public enum TestColor
{
    Red = 1,
    Green = 2,
    Blue = 3
}

[Trait("Category", "Unit")]
public class StrictEnumConverterTests
{
    private readonly JsonSerializerOptions _options;

    public StrictEnumConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new StrictEnumConverter());
    }

    [Fact]
    public void Deserialize_WithValidString_ShouldReturnEnumValue()
    {
        // Act
        var result = JsonSerializer.Deserialize<TestColor>("\"Green\"", _options);

        // Assert
        result.Should().Be(TestColor.Green);
    }

    [Fact]
    public void Deserialize_WithValidNumber_ShouldReturnEnumValue()
    {
        // Act
        var result = JsonSerializer.Deserialize<TestColor>("2", _options);

        // Assert
        result.Should().Be(TestColor.Green);
    }

    [Fact]
    public void Deserialize_WithInvalidString_ShouldThrowJsonException()
    {
        // Act & Assert
        Action act = () => JsonSerializer.Deserialize<TestColor>("\"Yellow\"", _options);
        act.Should().Throw<JsonException>().WithMessage("*não é um valor válido*");
    }

    [Fact]
    public void Deserialize_WithInvalidNumber_ShouldThrowJsonException()
    {
        // Act & Assert
        Action act = () => JsonSerializer.Deserialize<TestColor>("99", _options);
        act.Should().Throw<JsonException>().WithMessage("*não é um valor válido*");
    }

    [Fact]
    public void Deserialize_WithEmptyString_ShouldThrowJsonException()
    {
        // Act & Assert
        Action act = () => JsonSerializer.Deserialize<TestColor>("\"\"", _options);
        act.Should().Throw<JsonException>().WithMessage("*String vazia*");
    }

    [Fact]
    public void Deserialize_WhenIntegersNotAllowed_ShouldThrowJsonException()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new StrictEnumConverter(allowIntegerValues: false));

        // Act & Assert
        Action act = () => JsonSerializer.Deserialize<TestColor>("2", options);
        act.Should().Throw<JsonException>().WithMessage("*Valores inteiros não são permitidos*");
    }

    [Fact]
    public void Serialize_ShouldReturnStringValue()
    {
        // Act
        var result = JsonSerializer.Serialize(TestColor.Blue, _options);

        // Assert
        result.Should().Be("\"Blue\"");
    }
}
