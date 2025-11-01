using MeAjudaAi.Modules.Providers.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.ValueObjects;

public class QualificationTests
{
    [Fact]
    public void Constructor_WithValidName_ShouldCreateQualification()
    {
        // Arrange
        var name = "Professional Certification";

        // Act
        var qualification = new Qualification(name);

        // Assert
        qualification.Name.Should().Be(name);
        qualification.Description.Should().BeNull();
        qualification.IssuingOrganization.Should().BeNull();
        qualification.IssueDate.Should().BeNull();
        qualification.ExpirationDate.Should().BeNull();
        qualification.DocumentNumber.Should().BeNull();
        qualification.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateCompleteQualification()
    {
        // Arrange
        var name = "Professional Certification";
        var description = "A professional certification";
        var issuingOrganization = "Professional Board";
        var issueDate = DateTime.UtcNow.AddYears(-1);
        var expirationDate = DateTime.UtcNow.AddYears(1);
        var documentNumber = "CERT123456";

        // Act
        var qualification = new Qualification(
            name,
            description,
            issuingOrganization,
            issueDate,
            expirationDate,
            documentNumber);

        // Assert
        qualification.Name.Should().Be(name);
        qualification.Description.Should().Be(description);
        qualification.IssuingOrganization.Should().Be(issuingOrganization);
        qualification.IssueDate.Should().Be(issueDate);
        qualification.ExpirationDate.Should().Be(expirationDate);
        qualification.DocumentNumber.Should().Be(documentNumber);
        qualification.IsExpired.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Act & Assert
        var action = () => new Qualification(invalidName);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Qualification name cannot be empty*");
    }

    [Fact]
    public void IsExpired_WithPastExpirationDate_ShouldReturnTrue()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddDays(-1);
        var qualification = new Qualification(
            "Test Qualification",
            expirationDate: expirationDate);

        // Act & Assert
        qualification.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WithFutureExpirationDate_ShouldReturnFalse()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddDays(1);
        var qualification = new Qualification(
            "Test Qualification",
            expirationDate: expirationDate);

        // Act & Assert
        qualification.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithNoExpirationDate_ShouldReturnFalse()
    {
        // Arrange
        var qualification = new Qualification("Test Qualification");

        // Act & Assert
        qualification.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var issueDate = new DateTime(2023, 1, 1);
        var expirationDate = new DateTime(2025, 1, 1);

        var qualification1 = new Qualification(
            "Test Qualification",
            "Description",
            "Organization",
            issueDate,
            expirationDate,
            "DOC123");

        var qualification2 = new Qualification(
            "Test Qualification",
            "Description",
            "Organization",
            issueDate,
            expirationDate,
            "DOC123");

        // Act & Assert
        qualification1.Should().Be(qualification2);
        qualification1.GetHashCode().Should().Be(qualification2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentNames_ShouldNotBeEqual()
    {
        // Arrange
        var qualification1 = new Qualification("Qualification A");
        var qualification2 = new Qualification("Qualification B");

        // Act & Assert
        qualification1.Should().NotBe(qualification2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var qualification = new Qualification(
            "Professional Certification",
            issuingOrganization: "Professional Board",
            issueDate: new DateTime(2023, 1, 1),
            expirationDate: new DateTime(2025, 1, 1));

        // Act
        var result = qualification.ToString();

        // Assert
        result.Should().Contain("Professional Certification");
        result.Should().Contain("Professional Board");
        result.Should().Contain("1/1/2023");
        result.Should().Contain("1/1/2025");
    }
}
