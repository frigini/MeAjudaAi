using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;

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

        // Assert
        result.Should().Be(emailValue);
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
        email1.Should().NotBe(email3);
    }
}
