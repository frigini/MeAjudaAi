using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Constants;

namespace MeAjudaAi.Modules.Users.Tests.Unit.ValueObjects;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
public class EmailTests
{
    [Fact]
    public void Email_WithValidEmail_ShouldCreate()
    {
        // Arrange
        var validEmail = "test@example.com";

        // Act
        var email = new Email(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Email_WithMixedCase_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var mixedCaseEmail = "Test@Example.COM";

        // Act
        var email = new Email(mixedCaseEmail);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Email_ExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var domain = "@example.com";
        var tooLongLocalPart = new string('a', ValidationConstants.UserLimits.EmailMaxLength + 1 - domain.Length);
        var tooLongEmail = tooLongLocalPart + domain;

        // Act
        var act = () => new Email(tooLongEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email não pode ter mais de*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Email_WithInvalidEmail_ShouldThrow(string? invalidEmail)
    {
        // Arrange & Act
        var act = () => new Email(invalidEmail!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid@.com")]
    public void Email_WithMalformedEmail_ShouldThrow(string malformedEmail)
    {
        // Arrange & Act
        var act = () => new Email(malformedEmail);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Email_ToString_ShouldReturnValue()
    {
        // Arrange
        var emailValue = "test@example.com";
        var email = new Email(emailValue);

        // Act
        var result = email.ToString();

        // Assert - Records generate ToString() in format: TypeName { Property = Value }
        result.Should().Be($"Email {{ Value = {emailValue} }}");
    }

    [Fact]
    public void Email_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");
        var email3 = new Email("other@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.Equals(email2).Should().BeTrue();
        (email1 == email2).Should().BeTrue();
        (email1 != email3).Should().BeTrue();
        email1.GetHashCode().Should().Be(email2.GetHashCode());
        email1.Should().NotBe(email3);
    }

    [Fact]
    public void Email_Equality_WithDifferentCase_ShouldBeEqual()
    {
        // Arrange
        var email1 = new Email("Test@Example.COM");
        var email2 = new Email("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.Equals(email2).Should().BeTrue();
        (email1 == email2).Should().BeTrue();
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }
}
