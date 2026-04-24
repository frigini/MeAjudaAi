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
}
