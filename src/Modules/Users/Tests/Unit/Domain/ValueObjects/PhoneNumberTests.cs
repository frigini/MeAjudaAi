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
    [InlineData("123456789")]   // 9 dígitos - abaixo do mínimo BR (10)
    [InlineData("12345")]       // 5 dígitos - abaixo do mínimo BR (10)
    public void PhoneNumber_WithTooFewDigits_ShouldThrowArgumentException(string value)
    {
        // Act & Assert - BR requer 10-11 dígitos
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(value));
        exception.Message.Should().Be("Telefone brasileiro deve ter 10 ou 11 dígitos (DDD + número)");
    }

    [Fact]
    public void PhoneNumber_WithTooManyDigits_ShouldThrowArgumentException()
    {
        // Arrange - 12 dígitos (acima do máximo BR de 11)
        const string value = "123456789012";

        // Act & Assert - BR aceita no máximo 11 dígitos
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(value));
        exception.Message.Should().Be("Telefone brasileiro deve ter 10 ou 11 dígitos (DDD + número)");
    }

    [Theory]
    [InlineData("1234567890")]    // 10 dígitos - mínimo válido BR
    [InlineData("12345678901")]   // 11 dígitos - máximo válido BR
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
        exception.Message.Should().Contain("Código do país");
    }

    [Theory]
    [InlineData("12345678", "US")]    // 8 dígitos - mínimo válido non-BR
    [InlineData("123456789012345", "US")] // 15 dígitos - máximo válido non-BR
    public void PhoneNumber_NonBrazilian_AtBoundaryDigitCounts_ShouldCreateSuccessfully(string value, string countryCode)
    {
        // Act
        var phoneNumber = new PhoneNumber(value, countryCode);

        // Assert
        phoneNumber.Value.Should().Be(value);
        phoneNumber.CountryCode.Should().Be(countryCode);
    }

    [Theory]
    [InlineData("1234567", "US")]     // 7 dígitos - abaixo do mínimo
    [InlineData("1234567890123456", "US")] // 16 dígitos - acima do máximo
    public void PhoneNumber_NonBrazilian_OutOfRange_ShouldThrowArgumentException(string value, string countryCode)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PhoneNumber(value, countryCode));
    }

    [Theory]
    [InlineData("123")]      // Números em vez de letras
    [InlineData("B")]        // Apenas 1 letra
    [InlineData("BRA")]      // 3 letras (ISO alpha-3)
    [InlineData("B1")]       // Letra + número
    public void PhoneNumber_WithInvalidCountryCodeFormat_ShouldThrowArgumentException(string invalidCountryCode)
    {
        // Arrange
        const string value = "11999999999";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(value, invalidCountryCode));
        exception.Message.Should().Contain("ISO");
    }

    [Theory]
    [InlineData("+5511987654321", "11987654321", "BR")]  // Celular brasileiro
    [InlineData("+551133334444", "1133334444", "BR")]    // Fixo brasileiro
    public void PhoneNumber_WithBrazilianInternationalFormat_ShouldParseCorrectly(
        string input, string expectedValue, string expectedCountryCode)
    {
        // Act
        var phoneNumber = new PhoneNumber(input);

        // Assert
        phoneNumber.Value.Should().Be(expectedValue);
        phoneNumber.CountryCode.Should().Be(expectedCountryCode);
    }

    [Theory]
    [InlineData("+12125551234")]     // Número EUA
    [InlineData("+447911123456")]    // Número UK
    public void PhoneNumber_WithNonBrazilianInternationalFormat_ShouldUseGenericCountryCode(string input)
    {
        // Act
        var phoneNumber = new PhoneNumber(input);

        // Assert
        phoneNumber.CountryCode.Should().Be("XX");
        phoneNumber.Value.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("+55123")]           // Poucos dígitos após +55
    [InlineData("+55123456789012")]  // Muitos dígitos após +55
    public void PhoneNumber_WithInvalidBrazilianInternationalFormat_ShouldThrow(string input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PhoneNumber(input));
    }

    [Theory]
    [InlineData("+1234567")]         // Poucos dígitos totais (7)
    [InlineData("+1234567890123456")] // Muitos dígitos totais (16)
    public void PhoneNumber_WithInvalidInternationalFormat_ShouldThrow(string input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PhoneNumber(input));
    }

    [Theory]
    [InlineData("01987654321")]  // DDD inválido começando com 0
    public void PhoneNumber_WithInvalidBrazilianDDD_ShouldThrow(string value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(value, "BR"));
        exception.Message.Should().Contain("DDD");
    }

    [Theory]
    [InlineData("11887654321")]  // 11 dígitos sem 9 como terceiro dígito
    public void PhoneNumber_WithInvalidBrazilianMobileFormat_ShouldThrow(string value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(value, "BR"));
        exception.Message.Should().Contain("Celular brasileiro inválido");
    }

    [Theory]
    [InlineData("1198765432")]   // 10 dígitos com 9 como terceiro dígito
    public void PhoneNumber_WithInvalidBrazilianLandlineFormat_ShouldThrow(string value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(value, "BR"));
        exception.Message.Should().Contain("Telefone fixo brasileiro inválido");
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
