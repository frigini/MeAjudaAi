using FluentAssertions;
using MeAjudaAi.Shared.Utilities;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

[Trait("Category", "Unit")]
public class PhoneNumberValidatorTests
{
    [Theory]
    [InlineData("+5511999999999")]
    [InlineData("+12025550123")]
    [InlineData("+442071234567")]
    [InlineData("+55 11 99999-9999")]
    [InlineData("+55-11-99999-9999")]
    [InlineData("+55.11.99999.9999")] // Now valid with normalized dots
    public void IsValidInternationalFormat_WithValidNumbers_ShouldReturnTrue(string phoneNumber)
    {
        // Act
        var result = PhoneNumberValidator.IsValidInternationalFormat(phoneNumber);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("5511999999999")] // Missing +
    [InlineData("+55")] // Too short
    [InlineData("+1234567890123456")] // Too long (16 digits)
    [InlineData("+551199999a999")] // Contains letter
    [InlineData("+551199999!999")] // Contains invalid special char
    public void IsValidInternationalFormat_WithInvalidNumbers_ShouldReturnFalse(string? phoneNumber)
    {
        // Act
        var result = PhoneNumberValidator.IsValidInternationalFormat(phoneNumber);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("+5511999999999")]
    [InlineData("+55 11 99999-9999")]
    public void IsValid_Should_ReturnTrue_For_ValidBrazilianNumber(string phoneNumber)
    {
        // Act
        var result = PhoneNumberValidator.IsValidInternationalFormat(phoneNumber);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("11999999999")]
    [InlineData("invalid")]
    public void IsValid_Should_ReturnFalse_For_InvalidNumber(string? phoneNumber)
    {
        // Act
        var result = PhoneNumberValidator.IsValidInternationalFormat(phoneNumber);

        // Assert
        result.Should().BeFalse();
    }
}
