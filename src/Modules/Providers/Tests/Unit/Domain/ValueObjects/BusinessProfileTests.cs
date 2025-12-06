using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.ValueObjects;

public class BusinessProfileTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateBusinessProfile()
    {
        // Arrange
        var legalName = "Test Company Ltda";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        // Act
        var profile = new BusinessProfile(legalName, contactInfo, address);

        // Assert
        profile.LegalName.Should().Be(legalName);
        profile.ContactInfo.Should().Be(contactInfo);
        profile.PrimaryAddress.Should().Be(address);
        profile.FantasyName.Should().BeNull();
        profile.Description.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateBusinessProfile()
    {
        // Arrange
        var legalName = "Test Company Ltda";
        var fantasyName = "Test Co.";
        var description = "A test company";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        // Act
        var profile = new BusinessProfile(legalName, contactInfo, address, fantasyName, description);

        // Assert
        profile.LegalName.Should().Be(legalName);
        profile.FantasyName.Should().Be(fantasyName);
        profile.Description.Should().Be(description);
        profile.ContactInfo.Should().Be(contactInfo);
        profile.PrimaryAddress.Should().Be(address);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceLegalName_ShouldThrowArgumentException(string legalName)
    {
        // Arrange
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        // Act
        var act = () => new BusinessProfile(legalName, contactInfo, address);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Legal name cannot be empty*")
            .And.ParamName.Should().Be("legalName");
    }

    [Fact]
    public void Constructor_WithNullContactInfo_ShouldThrowArgumentNullException()
    {
        // Arrange
        var address = CreateValidAddress();

        // Act
        var act = () => new BusinessProfile("Test Company", null!, address);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("contactInfo");
    }

    [Fact]
    public void Constructor_WithNullAddress_ShouldThrowArgumentNullException()
    {
        // Arrange
        var contactInfo = new ContactInfo("test@example.com");

        // Act
        var act = () => new BusinessProfile("Test Company", contactInfo, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("primaryAddress");
    }

    [Fact]
    public void Constructor_ShouldTrimLegalName()
    {
        // Arrange
        var legalName = "  Test Company Ltda  ";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        // Act
        var profile = new BusinessProfile(legalName, contactInfo, address);

        // Assert
        profile.LegalName.Should().Be("Test Company Ltda");
    }

    [Fact]
    public void Constructor_ShouldTrimFantasyName()
    {
        // Arrange
        var fantasyName = "  Test Co.  ";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        // Act
        var profile = new BusinessProfile("Test Company", contactInfo, address, fantasyName);

        // Assert
        profile.FantasyName.Should().Be("Test Co.");
    }

    [Fact]
    public void Constructor_ShouldTrimDescription()
    {
        // Arrange
        var description = "  A test company  ";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        // Act
        var profile = new BusinessProfile("Test Company", contactInfo, address, description: description);

        // Assert
        profile.Description.Should().Be("A test company");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var legalName = "Test Company Ltda";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        var profile1 = new BusinessProfile(legalName, contactInfo, address);
        var profile2 = new BusinessProfile(legalName, contactInfo, address);

        // Act & Assert
        profile1.Should().Be(profile2);
        profile1.Equals(profile2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentLegalName_ShouldReturnFalse()
    {
        // Arrange
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        var profile1 = new BusinessProfile("Company A", contactInfo, address);
        var profile2 = new BusinessProfile("Company B", contactInfo, address);

        // Act & Assert
        profile1.Should().NotBe(profile2);
    }

    [Fact]
    public void Equals_WithDifferentContactInfo_ShouldReturnFalse()
    {
        // Arrange
        var address = CreateValidAddress();

        var profile1 = new BusinessProfile("Test Company", new ContactInfo("test1@example.com"), address);
        var profile2 = new BusinessProfile("Test Company", new ContactInfo("test2@example.com"), address);

        // Act & Assert
        profile1.Should().NotBe(profile2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHash()
    {
        // Arrange
        var legalName = "Test Company Ltda";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        var profile1 = new BusinessProfile(legalName, contactInfo, address);
        var profile2 = new BusinessProfile(legalName, contactInfo, address);

        // Act & Assert
        profile1.GetHashCode().Should().Be(profile2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var legalName = "Test Company Ltda";
        var fantasyName = "Test Co.";
        var description = "A test company";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        var profile = new BusinessProfile(legalName, contactInfo, address, fantasyName, description);

        // Act
        var result = profile.ToString();

        // Assert
        result.Should().Contain(legalName);
        result.Should().Contain(fantasyName);
        result.Should().Contain(description);
    }

    [Fact]
    public void ToString_WithNullOptionalFields_ShouldHandleGracefully()
    {
        // Arrange
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        var profile = new BusinessProfile("Test Company", contactInfo, address);

        // Act
        var result = profile.ToString();

        // Assert
        result.Should().Contain("Test Company");
        result.Should().NotBeNull();
    }

    private static Address CreateValidAddress()
    {
        return new Address(
            street: "Rua Test",
            number: "123",
            neighborhood: "Centro",
            city: "Teste",
            state: "TS",
            zipCode: "12345-678"
        );
    }
}
