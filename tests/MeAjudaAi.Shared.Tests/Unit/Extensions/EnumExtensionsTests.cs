using FluentAssertions;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Shared.Tests.Unit.Extensions;

public class EnumExtensionsTests
{
    private enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }

    [Fact]
    public void ToEnum_WithValidValue_ShouldReturnSuccessResult()
    {
        // Arrange
        var value = "Value1";

        // Act
        var result = value.ToEnum<TestEnum>();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TestEnum.Value1);
    }

    [Fact]
    public void ToEnum_WithValidValueDifferentCase_ShouldReturnSuccessResult()
    {
        // Arrange
        var value = "value2";

        // Act
        var result = value.ToEnum<TestEnum>();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TestEnum.Value2);
    }

    [Fact]
    public void ToEnum_WithIgnoreCaseFalse_ShouldBeCaseSensitive()
    {
        // Arrange
        var value = "value1";

        // Act
        var result = value.ToEnum<TestEnum>(ignoreCase: false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Invalid TestEnum");
    }

    [Fact]
    public void ToEnum_WithInvalidValue_ShouldReturnFailureResult()
    {
        // Arrange
        var value = "InvalidValue";

        // Act
        var result = value.ToEnum<TestEnum>();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("Invalid TestEnum");
        result.Error.Message.Should().Contain("Value1, Value2, Value3");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToEnum_WithNullOrWhiteSpace_ShouldReturnFailureResult(string? value)
    {
        // Act
        var result = EnumExtensions.ToEnum<TestEnum>(value);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("cannot be null or empty");
    }

    [Fact]
    public void ToEnumOrDefault_WithValidValue_ShouldReturnParsedEnum()
    {
        // Arrange
        var value = "Value2";

        // Act
        var result = value.ToEnumOrDefault(TestEnum.Value1);

        // Assert
        result.Should().Be(TestEnum.Value2);
    }

    [Fact]
    public void ToEnumOrDefault_WithInvalidValue_ShouldReturnDefaultValue()
    {
        // Arrange
        var value = "InvalidValue";

        // Act
        var result = value.ToEnumOrDefault(TestEnum.Value3);

        // Assert
        result.Should().Be(TestEnum.Value3);
    }

    [Fact]
    public void ToEnumOrDefault_WithNullValue_ShouldReturnDefaultValue()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value!.ToEnumOrDefault(TestEnum.Value1);

        // Assert
        result.Should().Be(TestEnum.Value1);
    }

    [Fact]
    public void ToEnumOrDefault_WithDifferentCase_ShouldReturnParsedEnum()
    {
        // Arrange
        var value = "value3";

        // Act
        var result = value.ToEnumOrDefault(TestEnum.Value1);

        // Assert
        result.Should().Be(TestEnum.Value3);
    }

    [Fact]
    public void IsValidEnum_WithValidValue_ShouldReturnTrue()
    {
        // Arrange
        var value = "Value1";

        // Act
        var result = value.IsValidEnum<TestEnum>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidEnum_WithInvalidValue_ShouldReturnFalse()
    {
        // Arrange
        var value = "InvalidValue";

        // Act
        var result = value.IsValidEnum<TestEnum>();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidEnum_WithNullValue_ShouldReturnFalse()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value!.IsValidEnum<TestEnum>();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidEnum_WithDifferentCase_ShouldReturnTrue()
    {
        // Arrange
        var value = "value2";

        // Act
        var result = value.IsValidEnum<TestEnum>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetValidValues_NoFilter_ShouldReturnAllEnumNames()
    {
        // Act
        var result = EnumExtensions.GetValidValues<TestEnum>();

        // Assert
        result.Should().BeEquivalentTo(["Value1", "Value2", "Value3"]);
    }

    [Fact]
    public void GetValidValuesDescription_NoFilter_ShouldReturnFormattedString()
    {
        // Act
        var result = EnumExtensions.GetValidValuesDescription<TestEnum>();

        // Assert
        result.Should().Contain("Valid TestEnum values:");
        result.Should().Contain("Value1");
        result.Should().Contain("Value2");
        result.Should().Contain("Value3");
    }
}
