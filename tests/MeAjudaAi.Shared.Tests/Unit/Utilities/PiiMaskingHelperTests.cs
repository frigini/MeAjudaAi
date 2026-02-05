using FluentAssertions;
using MeAjudaAi.Shared.Utilities;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

public class PiiMaskingHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void MaskUserId_WhenNullOrEmpty_ShouldReturnEmptyMessage(string? input)
    {
        var result = PiiMaskingHelper.MaskUserId(input);
        result.Should().Be("[EMPTY]");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("123456")]
    public void MaskUserId_WhenShort_ShouldMaskBetweenFirstAndLast(string input)
    {
        var result = PiiMaskingHelper.MaskUserId(input);
        result.Should().Be($"{input[0]}***{input[^1]}");
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("1234567890")]
    [InlineData("user-id-very-long")]
    public void MaskUserId_WhenLong_ShouldKeepFirstThreeAndLastThree(string input)
    {
        var result = PiiMaskingHelper.MaskUserId(input);
        
        result.Should().Be($"{input[..3]}***{input[^3..]}");
    }
}
