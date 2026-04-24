using MeAjudaAi.Shared.Utilities;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

[Trait("Category", "Unit")]
public class PiiMaskingHelperTests
{
    [Theory]
    [InlineData("user@example.com", "[REDACTED]")]
    [InlineData("123.456.789-00", "[REDACTED]")]
    [InlineData("+55 11 99999-9999", "[REDACTED]")]
    public void MaskSensitiveData_Should_ReturnRedacted_ForNonNullData(string input, string expected)
    {
        // Act
        var result = PiiMaskingHelper.MaskSensitiveData(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, "[EMPTY]")]
    [InlineData("", "[EMPTY]")]
    [InlineData("  ", "[EMPTY]")]
    public void MaskSensitiveData_Should_ReturnEmpty_ForNullOrWhitespaceData(string? input, string expected)
    {
        // Act
        var result = PiiMaskingHelper.MaskSensitiveData(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("123456789", "123***789")]
    [InlineData("12345", "1***5")]
    [InlineData(null, "[EMPTY]")]
    public void MaskUserId_Should_ReturnMaskedId(string? input, string expected)
    {
        // Act
        var result = PiiMaskingHelper.MaskUserId(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("johndoe@example.com", "jo**@example.com")]
    [InlineData("ab@example.com", "*@example.com")]
    [InlineData("invalid-email", "***@***")]
    [InlineData(null, "[EMPTY]")]
    public void MaskEmail_Should_ReturnMaskedEmail(string? input, string expected)
    {
        // Act
        var result = PiiMaskingHelper.MaskEmail(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("+5511999991234", "+5511****1234")]
    [InlineData("1234567", "*****67")]
    [InlineData("123", "****")]
    [InlineData(null, "[EMPTY]")]
    public void MaskPhoneNumber_Should_ReturnMaskedPhone(string? input, string expected)
    {
        // Act
        var result = PiiMaskingHelper.MaskPhoneNumber(input);

        // Assert
        result.Should().Be(expected);
    }
}
