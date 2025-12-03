using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.ValueObjects;

public sealed class ContactInfoTests
{
    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateContactInfo()
    {
        // Arrange
        var email = "provider@example.com";

        // Act
        var contactInfo = new ContactInfo(email);

        // Assert
        contactInfo.Email.Should().Be(email);
        contactInfo.PhoneNumber.Should().BeNull();
        contactInfo.Website.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateContactInfo()
    {
        // Arrange
        var email = "provider@example.com";
        var phoneNumber = "+55 11 98765-4321";
        var website = "https://www.provider.com";

        // Act
        var contactInfo = new ContactInfo(email, phoneNumber, website);

        // Assert
        contactInfo.Email.Should().Be(email);
        contactInfo.PhoneNumber.Should().Be(phoneNumber);
        contactInfo.Website.Should().Be(website);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act
        var act = () => new ContactInfo(invalidEmail!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be empty*");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("no-at-sign.com")]
    public void Constructor_WithInvalidEmailFormat_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act
        var act = () => new ContactInfo(invalidEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid email format*");
    }

    [Fact]
    public void Constructor_ShouldTrimWhitespace()
    {
        // Arrange
        var email = "  provider@example.com  ";
        var phoneNumber = "  +55 11 98765-4321  ";
        var website = "  https://www.provider.com  ";

        // Act
        var contactInfo = new ContactInfo(email, phoneNumber, website);

        // Assert
        contactInfo.Email.Should().Be("provider@example.com");
        contactInfo.PhoneNumber.Should().Be("+55 11 98765-4321");
        contactInfo.Website.Should().Be("https://www.provider.com");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var email = "provider@example.com";
        var phoneNumber = "+55 11 98765-4321";
        var website = "https://www.provider.com";
        var contactInfo = new ContactInfo(email, phoneNumber, website);

        // Act
        var result = contactInfo.ToString();

        // Assert
        result.Should().Contain(email);
        result.Should().Contain(phoneNumber);
        result.Should().Contain(website);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var email = "provider@example.com";
        var phoneNumber = "+55 11 98765-4321";
        var website = "https://www.provider.com";

        var contactInfo1 = new ContactInfo(email, phoneNumber, website);
        var contactInfo2 = new ContactInfo(email, phoneNumber, website);

        // Act & Assert
        contactInfo1.Should().Be(contactInfo2);
        (contactInfo1 == contactInfo2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEmails_ShouldReturnFalse()
    {
        // Arrange
        var contactInfo1 = new ContactInfo("provider1@example.com");
        var contactInfo2 = new ContactInfo("provider2@example.com");

        // Act & Assert
        contactInfo1.Should().NotBe(contactInfo2);
        (contactInfo1 != contactInfo2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var email = "provider@example.com";
        var phoneNumber = "+55 11 98765-4321";
        var website = "https://www.provider.com";

        var contactInfo1 = new ContactInfo(email, phoneNumber, website);
        var contactInfo2 = new ContactInfo(email, phoneNumber, website);

        // Act & Assert
        contactInfo1.GetHashCode().Should().Be(contactInfo2.GetHashCode());
    }
}
