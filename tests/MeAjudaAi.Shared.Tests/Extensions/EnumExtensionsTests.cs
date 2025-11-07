using FluentAssertions;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Functional;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Testes para EnumExtensions
/// </summary>
public class EnumExtensionsTests
{
    /// <summary>
    /// Enum para testes
    /// </summary>
    public enum TestEnum
    {
        None = 0,
        Value1 = 1,
        Value2 = 2,
        Value3 = 3
    }

    public class ToEnumTests
    {
        [Theory]
        [InlineData("Value1", TestEnum.Value1)]
        [InlineData("Value2", TestEnum.Value2)]
        [InlineData("Value3", TestEnum.Value3)]
        [InlineData("1", TestEnum.Value1)]
        [InlineData("2", TestEnum.Value2)]
        [InlineData("3", TestEnum.Value3)]
        public void ToEnum_WithValidValues_ShouldReturnSuccessResult(string value, TestEnum expected)
        {
            // Act
            var result = value.ToEnum<TestEnum>();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("InvalidValue")]
        [InlineData("99")]
        public void ToEnum_WithInvalidValues_ShouldReturnFailureResult(string value)
        {
            // Act
            var result = value.ToEnum<TestEnum>();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            
            if (string.IsNullOrWhiteSpace(value))
            {
                result.Error.Message.Should().StartWith("Value cannot be null or empty");
            }
            else
            {
                result.Error.Message.Should().Contain("Invalid TestEnum");
                result.Error.Message.Should().Contain("Valid values are:");
            }
        }

        [Fact]
        public void ToEnum_WithNullValue_ShouldReturnFailureResult()
        {
            // Act
            var result = ((string)null!).ToEnum<TestEnum>();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().StartWith("Value cannot be null or empty");
        }

        [Theory]
        [InlineData("value1", TestEnum.Value1)]
        [InlineData("VALUE2", TestEnum.Value2)]
        [InlineData("vAlUe3", TestEnum.Value3)]
        public void ToEnum_WithDifferentCasing_ShouldReturnSuccessResult(string value, TestEnum expected)
        {
            // Act
            var result = value.ToEnum<TestEnum>();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expected);
        }
    }

    public class ToEnumOrDefaultTests
    {
        [Theory]
        [InlineData("Value1", TestEnum.Value1)]
        [InlineData("Value2", TestEnum.Value2)]
        [InlineData("1", TestEnum.Value1)]
        public void ToEnumOrDefault_WithValidValues_ShouldReturnEnum(string value, TestEnum expected)
        {
            // Act
            var result = value.ToEnumOrDefault(TestEnum.Value1);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData("InvalidValue")]
        [InlineData("99")]
        public void ToEnumOrDefault_WithInvalidValues_ShouldReturnDefault(string value)
        {
            // Act
            var result = value.ToEnumOrDefault(TestEnum.Value1);

            // Assert
            result.Should().Be(TestEnum.Value1);
        }

        [Fact]
        public void ToEnumOrDefault_WithNullValue_ShouldReturnDefault()
        {
            // Act
            var result = ((string)null!).ToEnumOrDefault(TestEnum.Value2);

            // Assert
            result.Should().Be(TestEnum.Value2);
        }

        [Fact]
        public void ToEnumOrDefault_WithCustomDefault_ShouldReturnCustomDefault()
        {
            // Act
            var result = "InvalidValue".ToEnumOrDefault(TestEnum.Value3);

            // Assert
            result.Should().Be(TestEnum.Value3);
        }
    }

    public class IsValidEnumTests
    {
        [Theory]
        [InlineData("Value1", true)]
        [InlineData("Value2", true)]
        [InlineData("1", true)]
        [InlineData("2", true)]
        [InlineData("value1", true)] // Case insensitive
        [InlineData("InvalidValue", false)]
        [InlineData("99", false)]
        [InlineData("", false)]
        public void IsValidEnum_ShouldReturnCorrectResult(string value, bool expected)
        {
            // Act
            var result = value.IsValidEnum<TestEnum>();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void IsValidEnum_WithNullValue_ShouldReturnFalse()
        {
            // Act
            var result = ((string)null!).IsValidEnum<TestEnum>();

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("VALUE1", true, true)]
        [InlineData("VALUE1", false, false)]
        [InlineData("value1", true, true)]
        [InlineData("value1", false, false)] // case sensitive should be false
        public void IsValidEnum_WithIgnoreCase_ShouldRespectCaseSensitivity(string value, bool ignoreCase, bool expected)
        {
            // Act
            var result = value.IsValidEnum<TestEnum>(ignoreCase);

            // Assert
            result.Should().Be(expected);
        }
    }

    public class GetValidValuesTests
    {
        [Fact]
        public void GetValidValues_ShouldReturnAllEnumNames()
        {
            // Act
            var result = EnumExtensions.GetValidValues<TestEnum>();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result.Should().Contain("None");
            result.Should().Contain("Value1");
            result.Should().Contain("Value2");
            result.Should().Contain("Value3");
        }
    }

    public class GetValidValuesDescriptionTests
    {
        [Fact]
        public void GetValidValuesDescription_ShouldReturnFormattedDescription()
        {
            // Act
            var result = EnumExtensions.GetValidValuesDescription<TestEnum>();

            // Assert
            result.Should().NotBeNull();
            result.Should().StartWith("Valid TestEnum values:");
            result.Should().Contain("None");
            result.Should().Contain("Value1");
            result.Should().Contain("Value2");
            result.Should().Contain("Value3");
        }
    }
}