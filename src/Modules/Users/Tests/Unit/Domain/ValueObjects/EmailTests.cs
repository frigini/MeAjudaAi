using MeAjudaAi.Modules.Users.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("firstname+lastname@example.com")]
    [InlineData("1234567890@example.com")]
    [InlineData("email@example-one.com")]
    [InlineData("_______@example.com")]
    [InlineData("test.email.with+symbol@example.com")]
    public void Constructor_WithValidEmail_ShouldCreateEmail(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithNullOrWhitespace_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act & Assert
        var act = () => new Email(invalidEmail!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email não pode ser vazio*");
    }

    [Fact]
    public void Constructor_WithTooLongEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // Total > 254 caracteres

        // Act & Assert
        var act = () => new Email(longEmail);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email não pode ter mais de 254 caracteres*");
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missingdomain.com")]
    [InlineData("missing@.com")]
    [InlineData("missing@domain")]
    [InlineData("spaces @domain.com")]
    [InlineData("email@domain .com")]
    [InlineData("email@@domain.com")]
    public void Constructor_WithInvalidEmailFormat_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        var act = () => new Email(invalidEmail);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Formato de email inválido*");
    }

    [Fact]
    public void Constructor_ShouldConvertToLowerCase()
    {
        // Arrange
        var upperCaseEmail = "TEST@EXAMPLE.COM";

        // Act
        var email = new Email(upperCaseEmail);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitOperator_ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var emailValue = "test@example.com";
        var email = new Email(emailValue);

        // Act
        string result = email;

        // Assert
        result.Should().Be(emailValue);
    }

    [Fact]
    public void ImplicitOperator_FromString_ShouldCreateEmail()
    {
        // Arrange
        var emailValue = "test@example.com";

        // Act
        Email email = emailValue;

        // Assert
        email.Value.Should().Be(emailValue);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var emailValue = "test@example.com";
        var email1 = new Email(emailValue);
        var email2 = new Email(emailValue);

        // Act & Assert
        email1.Should().Be(email2);
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentCasing_ShouldReturnTrue()
    {
        // Arrange
        var email1 = new Email("TEST@EXAMPLE.COM");
        var email2 = new Email("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        email1.GetHashCode().Should().NotBe(email2.GetHashCode());
    }
}