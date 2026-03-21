using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using Xunit;
using System.ComponentModel.DataAnnotations;

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
        var showAddressToClient = true;

        // Act
        var profile = new BusinessProfile(legalName, contactInfo, address, fantasyName, description, showAddressToClient);

        // Assert
        profile.LegalName.Should().Be(legalName);
        profile.FantasyName.Should().Be(fantasyName);
        profile.Description.Should().Be(description);
        profile.ContactInfo.Should().Be(contactInfo);
        profile.PrimaryAddress.Should().Be(address);
        profile.ShowAddressToClient.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithDefaultParameters_ShouldSetShowAddressToClientToFalse()
    {
        // Arrange
        var legalName = "Test Company Ltda";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        // Act
        var profile = new BusinessProfile(legalName, contactInfo, address);

        // Assert
        profile.ShowAddressToClient.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var legalName = "Test Company Ltda";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        var profile1 = new BusinessProfile(legalName, contactInfo, address, showAddressToClient: true);
        var profile2 = new BusinessProfile(legalName, contactInfo, address, showAddressToClient: true);

        // Act & Assert
        profile1.Should().Be(profile2);
        profile1.Equals(profile2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentShowAddressToClient_ShouldReturnFalse()
    {
        // Arrange
        var legalName = "Test Company Ltda";
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        var profile1 = new BusinessProfile(legalName, contactInfo, address, showAddressToClient: true);
        var profile2 = new BusinessProfile(legalName, contactInfo, address, showAddressToClient: false);

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

        var profile1 = new BusinessProfile(legalName, contactInfo, address, showAddressToClient: true);
        var profile2 = new BusinessProfile(legalName, contactInfo, address, showAddressToClient: true);

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

    [Fact]
    public void Constructor_WithNullLegalName_ShouldThrowArgumentException()
    {
        // Arrange
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        // Act & Assert
        var act = () => new BusinessProfile(null!, contactInfo, address);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("legalName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceLegalName_ShouldThrowArgumentException(string invalidLegalName)
    {
        // Arrange
        var contactInfo = new ContactInfo("test@example.com");
        var address = CreateValidAddress();

        // Act & Assert
        var act = () => new BusinessProfile(invalidLegalName, contactInfo, address);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("legalName");
    }

    [Fact]
    public void Constructor_WithNullContactInfo_ShouldThrowArgumentNullException()
    {
        // Arrange
        var address = CreateValidAddress();

        // Act & Assert
        var act = () => new BusinessProfile("Test Company", null!, address);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("contactInfo");
    }

    [Fact]
    public void Constructor_WithNullPrimaryAddress_WhenShowAddressToClientFalse_ShouldWork()
    {
        // Arrange
        var contactInfo = new ContactInfo("test@example.com");

        // Act
        var profile = new BusinessProfile("Test Company", contactInfo, null!, null, null, showAddressToClient: false);

        // Assert
        profile.LegalName.Should().Be("Test Company");
        profile.PrimaryAddress.Should().BeNull();
        profile.ShowAddressToClient.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithNullPrimaryAddress_WhenShowAddressToClientTrue_ShouldThrow()
    {
        // Arrange
        var contactInfo = new ContactInfo("test@example.com");

        // Act & Assert
        var act = () => new BusinessProfile("Test Company", contactInfo, null!, null, null, showAddressToClient: true);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("primaryAddress");
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
