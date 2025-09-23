using MeAjudaAi.Modules.Users.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.ValueObjects;

public class PhoneNumberTests
{
    [Fact]
    public void PhoneNumber_WithValidValueAndCountryCode_ShouldCreateSuccessfully()
    {
        // Arrange
        const string value = "(11) 99999-9999";
        const string countryCode = "BR";

        // Act
        var phoneNumber = new PhoneNumber(value, countryCode);

        // Assert
        phoneNumber.Value.Should().Be(value);
        phoneNumber.CountryCode.Should().Be(countryCode);
    }

    [Fact]
    public void PhoneNumber_WithOnlyValue_ShouldUseDefaultCountryCode()
    {
        // Arrange
        const string value = "(11) 99999-9999";

        // Act
        var phoneNumber = new PhoneNumber(value);

        // Assert
        phoneNumber.Value.Should().Be(value);
        phoneNumber.CountryCode.Should().Be("BR");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PhoneNumber_WithInvalidValue_ShouldThrowArgumentException(string? invalidValue)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(invalidValue));
        exception.Message.Should().Be("Phone number cannot be empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PhoneNumber_WithInvalidCountryCode_ShouldThrowArgumentException(string? invalidCountryCode)
    {
        // Arrange
        const string value = "(11) 99999-9999";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(value, invalidCountryCode));
        exception.Message.Should().Be("Country code cannot be empty");
    }

    [Fact]
    public void PhoneNumber_WithWhitespaceInValue_ShouldTrimValue()
    {
        // Arrange
        const string value = "  (11) 99999-9999  ";
        const string countryCode = "  BR  ";

        // Act
        var phoneNumber = new PhoneNumber(value, countryCode);

        // Assert
        phoneNumber.Value.Should().Be("(11) 99999-9999");
        phoneNumber.CountryCode.Should().Be("BR");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedPhoneNumber()
    {
        // Arrange
        const string value = "(11) 99999-9999";
        const string countryCode = "BR";
        var phoneNumber = new PhoneNumber(value, countryCode);

        // Act
        var result = phoneNumber.ToString();

        // Assert
        result.Should().Be("BR (11) 99999-9999");
    }

    [Fact]
    public void PhoneNumbers_WithSameValueAndCountryCode_ShouldBeEqual()
    {
        // Arrange
        const string value = "(11) 99999-9999";
        const string countryCode = "BR";
        var phoneNumber1 = new PhoneNumber(value, countryCode);
        var phoneNumber2 = new PhoneNumber(value, countryCode);

        // Act & Assert
        phoneNumber1.Should().Be(phoneNumber2);
        phoneNumber1.GetHashCode().Should().Be(phoneNumber2.GetHashCode());
    }

    [Fact]
    public void PhoneNumbers_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var phoneNumber1 = new PhoneNumber("(11) 99999-9999", "BR");
        var phoneNumber2 = new PhoneNumber("(11) 88888-8888", "BR");

        // Act & Assert
        phoneNumber1.Should().NotBe(phoneNumber2);
    }

    [Fact]
    public void PhoneNumbers_WithDifferentCountryCodes_ShouldNotBeEqual()
    {
        // Arrange
        const string value = "(11) 99999-9999";
        var phoneNumber1 = new PhoneNumber(value, "BR");
        var phoneNumber2 = new PhoneNumber(value, "US");

        // Act & Assert
        phoneNumber1.Should().NotBe(phoneNumber2);
    }

    [Fact]
    public void PhoneNumber_ComparedWithNull_ShouldNotBeEqual()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("(11) 99999-9999");

        // Act & Assert
        phoneNumber.Should().NotBeNull();
        phoneNumber.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void PhoneNumber_ComparedWithDifferentType_ShouldNotBeEqual()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("(11) 99999-9999");
        var differentTypeObject = "not a phone number";

        // Act & Assert
        phoneNumber.Equals(differentTypeObject).Should().BeFalse();
    }
}