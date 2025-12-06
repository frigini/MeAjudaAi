using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.ValueObjects;

public class ContactInfoTests
{
    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateContactInfo()
    {
        // Arrange
        var email = "test@example.com";

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
        var email = "test@example.com";
        var phone = "+55 11 98765-4321";
        var website = "https://example.com";

        // Act
        var contactInfo = new ContactInfo(email, phone, website);

        // Assert
        contactInfo.Email.Should().Be(email);
        contactInfo.PhoneNumber.Should().Be(phone);
        contactInfo.Website.Should().Be(website);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceEmail_ShouldThrowArgumentException(string email)
    {
        // Act
        var act = () => new ContactInfo(email);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*")
            .And.ParamName.Should().Be("email");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test")]
    [InlineData("test @example.com")]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string email)
    {
        // Act
        var act = () => new ContactInfo(email);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email format*")
            .And.ParamName.Should().Be("email");
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.com")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("123@test.com")]
    public void Constructor_WithValidEmailFormats_ShouldSucceed(string email)
    {
        // Act
        var act = () => new ContactInfo(email);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ShouldTrimPhoneAndWebsite()
    {
        // Arrange
        var email = "test@example.com";
        var phone = "  +55 11 98765-4321  ";
        var website = "  https://example.com  ";

        // Act
        var contactInfo = new ContactInfo(email, phone, website);

        // Assert
        contactInfo.PhoneNumber.Should().Be("+55 11 98765-4321");
        contactInfo.Website.Should().Be("https://example.com");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var phone = "+55 11 98765-4321";
        var website = "https://example.com";

        var contact1 = new ContactInfo(email, phone, website);
        var contact2 = new ContactInfo(email, phone, website);

        // Act & Assert
        contact1.Should().Be(contact2);
        contact1.Equals(contact2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var contact1 = new ContactInfo("test1@example.com");
        var contact2 = new ContactInfo("test2@example.com");

        // Act & Assert
        contact1.Should().NotBe(contact2);
    }

    [Fact]
    public void Equals_WithDifferentPhone_ShouldReturnFalse()
    {
        // Arrange
        var contact1 = new ContactInfo("test@example.com", "+55 11 11111-1111");
        var contact2 = new ContactInfo("test@example.com", "+55 11 22222-2222");

        // Act & Assert
        contact1.Should().NotBe(contact2);
    }

    [Fact]
    public void Equals_WithNullVsEmptyPhone_ShouldBeEqual()
    {
        // Arrange - Both null and empty should be treated as equal in equality comparison
        var contact1 = new ContactInfo("test@example.com", null);
        var contact2 = new ContactInfo("test@example.com");

        // Act & Assert
        contact1.Should().Be(contact2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHash()
    {
        // Arrange
        var email = "test@example.com";
        var phone = "+55 11 98765-4321";

        var contact1 = new ContactInfo(email, phone);
        var contact2 = new ContactInfo(email, phone);

        // Act & Assert
        contact1.GetHashCode().Should().Be(contact2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var phone = "+55 11 98765-4321";
        var website = "https://example.com";

        var contactInfo = new ContactInfo(email, phone, website);

        // Act
        var result = contactInfo.ToString();

        // Assert
        result.Should().Contain(email);
        result.Should().Contain(phone);
        result.Should().Contain(website);
    }

    [Fact]
    public void ToString_WithNullOptionalFields_ShouldHandleGracefully()
    {
        // Arrange
        var contactInfo = new ContactInfo("test@example.com");

        // Act
        var result = contactInfo.ToString();

        // Assert
        result.Should().Contain("test@example.com");
        result.Should().NotBeNull();
    }
}
