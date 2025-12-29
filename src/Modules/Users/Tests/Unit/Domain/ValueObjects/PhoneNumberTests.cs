using MeAjudaAi.Modules.Users.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.ValueObjects;

[Trait("Category", "Unit")]
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
        phoneNumber.Value.Should().Be("11999999999"); // Apenas dígitos (11 dígitos)
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
        phoneNumber.Value.Should().Be("11999999999"); // Apenas dígitos (11 dígitos)
        phoneNumber.CountryCode.Should().Be("BR");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PhoneNumber_WithInvalidValue_ShouldThrowArgumentException(string? invalidValue)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(invalidValue!));
        exception.Message.Should().Be("Telefone não pode ser vazio");
    }

    [Theory]
    [InlineData("1234567")]   // 7 dígitos - abaixo do mínimo
    [InlineData("12345")]     // 5 dígitos - abaixo do mínimo
    public void PhoneNumber_WithTooFewDigits_ShouldThrowArgumentException(string value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(value));
        exception.Message.Should().Be("Telefone deve ter pelo menos 8 dígitos");
    }

    [Fact]
    public void PhoneNumber_WithTooManyDigits_ShouldThrowArgumentException()
    {
        // Arrange - 16 dígitos
        const string value = "1234567890123456";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(value));
        exception.Message.Should().Be("Telefone não pode ter mais de 15 dígitos");
    }

    [Theory]
    [InlineData("12345678")]         // 8 dígitos - mínimo válido
    [InlineData("123456789012345")]  // 15 dígitos - máximo válido
    public void PhoneNumber_AtBoundaryDigitCounts_ShouldCreateSuccessfully(string value)
    {
        // Act
        var phoneNumber = new PhoneNumber(value);

        // Assert
        phoneNumber.Value.Should().Be(value);
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
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(value, invalidCountryCode!));
        exception.Message.Should().Be("Código do país não pode ser vazio");
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
        phoneNumber.Value.Should().Be("11999999999"); // Apenas dígitos (11 dígitos)
        phoneNumber.CountryCode.Should().Be("BR");
    }

    [Fact]
    public void PhoneNumber_WithLowercaseCountryCode_ShouldNormalizeToUppercase()
    {
        // Arrange
        const string value = "11999999999";
        const string countryCode = "br";

        // Act
        var phoneNumber = new PhoneNumber(value, countryCode);

        // Assert
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
        result.Should().Be("BR 11999999999"); // CountryCode + espaço + dígitos (11 dígitos)
    }

    [Fact]
    public void PhoneNumbers_WithDifferentFormattingButSameDigits_ShouldBeEqual()
    {
        // Arrange
        var phoneNumber1 = new PhoneNumber("(11) 99999-9999", "BR");
        var phoneNumber2 = new PhoneNumber("11999999999", "BR");

        // Act & Assert
        phoneNumber1.Should().Be(phoneNumber2);
        phoneNumber1.GetHashCode().Should().Be(phoneNumber2.GetHashCode());
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
