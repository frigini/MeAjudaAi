using MeAjudaAi.Shared.Utilities;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

[Trait("Category", "Unit")]
public class PiiMaskingHelperTests
{
    [Theory]
    [InlineData("123456789", "123***789")]
    [InlineData("abcdef123", "abc***123")]
    [InlineData("abc", "a***c")]
    [InlineData("123456", "1***6")]
    [InlineData("1", "1***1")] // Special case for very short IDs
    [InlineData(null, "[EMPTY]")]
    [InlineData("", "[EMPTY]")]
    [InlineData("   ", "[EMPTY]")]
    public void MaskUserId_ShouldMaskCorrectly(string? input, string expected)
    {
        // Act
        var result = PiiMaskingHelper.MaskUserId(input);

        // Assert
        result.Should().Be(expected);
    }
}
