using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.ValueObjects;

public sealed class QualificationTests
{
    [Fact]
    public void Constructor_WithValidName_ShouldCreateQualification()
    {
        // Arrange
        var name = "Certified Public Accountant";

        // Act
        var qualification = new Qualification(name);

        // Assert
        qualification.Name.Should().Be(name);
        qualification.Description.Should().BeNull();
        qualification.IssuingOrganization.Should().BeNull();
        qualification.IssueDate.Should().BeNull();
        qualification.ExpirationDate.Should().BeNull();
        qualification.DocumentNumber.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateQualification()
    {
        // Arrange
        var name = "Certified Public Accountant";
        var description = "Professional accounting certification";
        var organization = "Brazilian Federal Accounting Council";
        var issueDate = new DateTime(2020, 1, 15);
        var expirationDate = new DateTime(2025, 1, 15);
        var documentNumber = "CRC-SP-123456";

        // Act
        var qualification = new Qualification(
            name,
            description,
            organization,
            issueDate,
            expirationDate,
            documentNumber);

        // Assert
        qualification.Name.Should().Be(name);
        qualification.Description.Should().Be(description);
        qualification.IssuingOrganization.Should().Be(organization);
        qualification.IssueDate.Should().Be(issueDate);
        qualification.ExpirationDate.Should().Be(expirationDate);
        qualification.DocumentNumber.Should().Be(documentNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        var act = () => new Qualification(invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Qualification name cannot be empty*");
    }

    [Fact]
    public void Constructor_WithExpirationBeforeIssue_ShouldThrowArgumentException()
    {
        // Arrange
        var issueDate = new DateTime(2025, 1, 15);
        var expirationDate = new DateTime(2020, 1, 15);

        // Act
        var act = () => new Qualification(
            "Test Qualification",
            issueDate: issueDate,
            expirationDate: expirationDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Expiration date cannot be before issue date*");
    }

    [Fact]
    public void IsExpiredAt_WithExpiredQualification_ShouldReturnTrue()
    {
        // Arrange
        var expirationDate = new DateTime(2020, 12, 31);
        var qualification = new Qualification(
            "Expired Certification",
            expirationDate: expirationDate);
        var referenceDate = new DateTime(2024, 1, 1);

        // Act
        var isExpired = qualification.IsExpiredAt(referenceDate);

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpiredAt_WithValidQualification_ShouldReturnFalse()
    {
        // Arrange
        var expirationDate = new DateTime(2025, 12, 31);
        var qualification = new Qualification(
            "Valid Certification",
            expirationDate: expirationDate);
        var referenceDate = new DateTime(2024, 1, 1);

        // Act
        var isExpired = qualification.IsExpiredAt(referenceDate);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpiredAt_WithNoExpirationDate_ShouldReturnFalse()
    {
        // Arrange
        var qualification = new Qualification("Permanent Certification");
        var referenceDate = DateTime.UtcNow;

        // Act
        var isExpired = qualification.IsExpiredAt(referenceDate);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldTrimWhitespace()
    {
        // Arrange
        var name = "  Test Certification  ";
        var description = "  Test Description  ";
        var organization = "  Test Org  ";
        var documentNumber = "  DOC-123  ";

        // Act
        var qualification = new Qualification(
            name,
            description,
            organization,
            documentNumber: documentNumber);

        // Assert
        qualification.Name.Should().Be("Test Certification");
        qualification.Description.Should().Be("Test Description");
        qualification.IssuingOrganization.Should().Be("Test Org");
        qualification.DocumentNumber.Should().Be("DOC-123");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var name = "Test Cert";
        var description = "Description";
        var organization = "Org";
        var issueDate = new DateTime(2020, 1, 1);
        var expirationDate = new DateTime(2025, 1, 1);
        var documentNumber = "DOC-123";

        var qualification1 = new Qualification(name, description, organization, issueDate, expirationDate, documentNumber);
        var qualification2 = new Qualification(name, description, organization, issueDate, expirationDate, documentNumber);

        // Act & Assert
        qualification1.Should().Be(qualification2);
    }

    [Fact]
    public void Equals_WithDifferentNames_ShouldReturnFalse()
    {
        // Arrange
        var qualification1 = new Qualification("Certification A");
        var qualification2 = new Qualification("Certification B");

        // Act & Assert
        qualification1.Should().NotBe(qualification2);
    }
}
