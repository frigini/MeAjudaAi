using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.Mocks;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.Entities;

/// <summary>
/// Testes adicionais para casos de borda e cenários não cobertos do Provider Entity.
/// Criado para aumentar coverage de 0% → 100%.
/// </summary>
[Trait("Category", "Unit")]
public class ProviderEdgeCasesTests
{
    private static BusinessProfile CreateValidBusinessProfile(
        string? email = null,
        string? legalName = null,
        string? fantasyName = null,
        string? description = null)
    {
        var address = new Address(
            street: "Rua Teste",
            number: "123",
            neighborhood: "Centro",
            city: "São Paulo",
            state: "SP",
            zipCode: "01234-567",
            country: "Brasil");

        var contactInfo = new ContactInfo(
            email: email ?? "test@provider.com",
            phoneNumber: "+55 11 99999-9999",
            website: "https://www.provider.com");

        return new BusinessProfile(
            legalName: legalName ?? "Provider Test LTDA",
            fantasyName: fantasyName,
            description: description,
            contactInfo: contactInfo,
            primaryAddress: address);
    }

    private static Provider CreateValidProvider()
    {
        var userId = Guid.NewGuid();
        var name = "Test Provider";
        var type = EProviderType.Individual;
        var businessProfile = CreateValidBusinessProfile();

        return new Provider(userId, name, type, businessProfile);
    }

    #region UpdateProfile Edge Cases - Tracking Field Changes

    [Fact]
    public void UpdateProfile_WhenOnlyNameChanged_ShouldTrackOnlyNameField()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newName = "Updated Provider Name";
        var originalProfile = provider.BusinessProfile;

        // Act
        provider.UpdateProfile(newName, originalProfile, "admin@test.com");

        // Assert
        var updateEvent = provider.DomainEvents.OfType<ProviderProfileUpdatedDomainEvent>().Last();
        updateEvent.UpdatedFields.Should().ContainSingle().Which.Should().Be("Name");
    }

    [Fact]
    public void UpdateProfile_WhenOnlyEmailChanged_ShouldTrackOnlyEmailField()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newProfile = CreateValidBusinessProfile(email: "newemail@provider.com");

        // Act
        provider.UpdateProfile(provider.Name, newProfile, "admin@test.com");

        // Assert
        var updateEvent = provider.DomainEvents.OfType<ProviderProfileUpdatedDomainEvent>().Last();
        updateEvent.UpdatedFields.Should().ContainSingle().Which.Should().Be("Email");
    }

    [Fact]
    public void UpdateProfile_WhenOnlyLegalNameChanged_ShouldTrackOnlyLegalNameField()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newProfile = CreateValidBusinessProfile(legalName: "New Legal Name LTDA");

        // Act
        provider.UpdateProfile(provider.Name, newProfile, "admin@test.com");

        // Assert
        var updateEvent = provider.DomainEvents.OfType<ProviderProfileUpdatedDomainEvent>().Last();
        updateEvent.UpdatedFields.Should().ContainSingle().Which.Should().Be("LegalName");
    }

    [Fact]
    public void UpdateProfile_WhenOnlyFantasyNameChanged_ShouldTrackOnlyFantasyNameField()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newProfile = CreateValidBusinessProfile(fantasyName: "New Fantasy Name");

        // Act
        provider.UpdateProfile(provider.Name, newProfile, "admin@test.com");

        // Assert
        var updateEvent = provider.DomainEvents.OfType<ProviderProfileUpdatedDomainEvent>().Last();
        updateEvent.UpdatedFields.Should().ContainSingle().Which.Should().Be("FantasyName");
    }

    [Fact]
    public void UpdateProfile_WhenOnlyDescriptionChanged_ShouldTrackOnlyDescriptionField()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newProfile = CreateValidBusinessProfile(description: "New description");

        // Act
        provider.UpdateProfile(provider.Name, newProfile, "admin@test.com");

        // Assert
        var updateEvent = provider.DomainEvents.OfType<ProviderProfileUpdatedDomainEvent>().Last();
        updateEvent.UpdatedFields.Should().ContainSingle().Which.Should().Be("Description");
    }

    [Fact]
    public void UpdateProfile_WhenMultipleFieldsChanged_ShouldTrackAllChangedFields()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newName = "Updated Name";
        var newProfile = CreateValidBusinessProfile(
            email: "newemail@provider.com",
            legalName: "New Legal Name LTDA");

        // Act
        provider.UpdateProfile(newName, newProfile, "admin@test.com");

        // Assert
        var updateEvent = provider.DomainEvents.OfType<ProviderProfileUpdatedDomainEvent>().Last();
        updateEvent.UpdatedFields.Should().Contain("Name");
        updateEvent.UpdatedFields.Should().Contain("Email");
        updateEvent.UpdatedFields.Should().Contain("LegalName");
    }

    [Fact]
    public void UpdateProfile_WhenNoFieldsChanged_ShouldTrackNoFields()
    {
        // Arrange
        var provider = CreateValidProvider();
        var originalProfile = provider.BusinessProfile;
        var originalName = provider.Name;

        // Act
        provider.UpdateProfile(originalName, originalProfile, "admin@test.com");

        // Assert
        var updateEvent = provider.DomainEvents.OfType<ProviderProfileUpdatedDomainEvent>().Last();
        updateEvent.UpdatedFields.Should().BeEmpty();
    }

    [Fact]
    public void UpdateProfile_WhenNameHasWhitespace_ShouldTrimAndCompare()
    {
        // Arrange
        var provider = CreateValidProvider();
        var nameWithWhitespace = "  Test Provider  "; // Same name but with whitespace

        // Act
        provider.UpdateProfile(nameWithWhitespace, provider.BusinessProfile, "admin@test.com");

        // Assert
        provider.Name.Should().Be("Test Provider"); // Trimmed
        var updateEvent = provider.DomainEvents.OfType<ProviderProfileUpdatedDomainEvent>().Last();
        updateEvent.UpdatedFields.Should().BeEmpty(); // No change detected
    }

    [Fact]
    public void UpdateProfile_WithNullBusinessProfile_ShouldThrowArgumentNullException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act & Assert
        var action = () => provider.UpdateProfile("New Name", null!, "admin@test.com");
        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void UpdateProfile_WithInvalidName_ShouldThrowProviderDomainException(string? invalidName)
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument - This is intentional for testing
        var action = () => provider.UpdateProfile(invalidName, provider.BusinessProfile, "admin@test.com");
#pragma warning restore CS8604
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Name cannot be empty");
    }

    #endregion

    #region Document Primary Status Tests

    [Fact]
    public void AddDocument_WithPrimaryDocument_ShouldUnsetOtherPrimaryDocuments()
    {
        // Arrange
        var provider = CreateValidProvider();
        var doc1 = new Document("11144477735", EDocumentType.CPF, isPrimary: true);
        var doc2 = new Document("12345678000195", EDocumentType.CNPJ, isPrimary: true);

        // Act
        provider.AddDocument(doc1);
        provider.AddDocument(doc2);

        // Assert
        provider.Documents.Should().HaveCount(2);
        provider.Documents.Count(d => d.IsPrimary).Should().Be(1);
        provider.GetPrimaryDocument().Should().NotBeNull();
        provider.GetPrimaryDocument()!.DocumentType.Should().Be(EDocumentType.CNPJ);
    }

    [Fact]
    public void AddDocument_WithNonPrimaryDocument_ShouldNotChangeExistingPrimaryDocument()
    {
        // Arrange
        var provider = CreateValidProvider();
        var doc1 = new Document("11144477735", EDocumentType.CPF, isPrimary: true);
        var doc2 = new Document("12345678000195", EDocumentType.CNPJ, isPrimary: false);

        // Act
        provider.AddDocument(doc1);
        provider.AddDocument(doc2);

        // Assert
        provider.GetPrimaryDocument()!.DocumentType.Should().Be(EDocumentType.CPF);
    }

    [Fact]
    public void SetPrimaryDocument_WithExistingDocument_ShouldSetAsPrimary()
    {
        // Arrange
        var provider = CreateValidProvider();
        var doc1 = new Document("11144477735", EDocumentType.CPF);
        var doc2 = new Document("12345678000195", EDocumentType.CNPJ);
        provider.AddDocument(doc1);
        provider.AddDocument(doc2);

        // Act
        provider.SetPrimaryDocument(EDocumentType.CNPJ);

        // Assert
        provider.GetPrimaryDocument()!.DocumentType.Should().Be(EDocumentType.CNPJ);
    }

    [Fact]
    public void SetPrimaryDocument_WithNonExistingDocument_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act & Assert
        var action = () => provider.SetPrimaryDocument(EDocumentType.CPF);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Document of type CPF not found");
    }

    [Fact]
    public void SetPrimaryDocument_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var document = new Document("11144477735", EDocumentType.CPF);
        provider.AddDocument(document);
        provider.Delete(new MockDateTimeProvider());

        // Act & Assert
        var action = () => provider.SetPrimaryDocument(EDocumentType.CPF);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot set primary document on deleted provider");
    }

    [Fact]
    public void GetPrimaryDocument_WhenNoPrimaryDocument_ShouldReturnNull()
    {
        // Arrange
        var provider = CreateValidProvider();
        var document = new Document("11144477735", EDocumentType.CPF, isPrimary: false);
        provider.AddDocument(document);

        // Act
        var result = provider.GetPrimaryDocument();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetPrimaryDocument_WhenHasPrimaryDocument_ShouldReturnIt()
    {
        // Arrange
        var provider = CreateValidProvider();
        var document = new Document("11144477735", EDocumentType.CPF, isPrimary: true);
        provider.AddDocument(document);

        // Act
        var result = provider.GetPrimaryDocument();

        // Assert
        result.Should().NotBeNull();
        result!.DocumentType.Should().Be(EDocumentType.CPF);
    }

    [Fact]
    public void GetMainDocument_WhenNoPrimaryButHasDocuments_ShouldReturnFirstDocument()
    {
        // Arrange
        var provider = CreateValidProvider();
        var doc1 = new Document("11144477735", EDocumentType.CPF, isPrimary: false);
        var doc2 = new Document("12345678000195", EDocumentType.CNPJ, isPrimary: false);
        provider.AddDocument(doc1);
        provider.AddDocument(doc2);

        // Act
        var result = provider.GetMainDocument();

        // Assert
        result.Should().NotBeNull();
        result!.DocumentType.Should().Be(EDocumentType.CPF); // First one added
    }

    [Fact]
    public void GetMainDocument_WhenHasPrimaryDocument_ShouldReturnPrimaryDocument()
    {
        // Arrange
        var provider = CreateValidProvider();
        var doc1 = new Document("11144477735", EDocumentType.CPF, isPrimary: false);
        var doc2 = new Document("12345678000195", EDocumentType.CNPJ, isPrimary: true);
        provider.AddDocument(doc1);
        provider.AddDocument(doc2);

        // Act
        var result = provider.GetMainDocument();

        // Assert
        result.Should().NotBeNull();
        result!.DocumentType.Should().Be(EDocumentType.CNPJ); // Primary document
    }

    [Fact]
    public void GetMainDocument_WhenNoDocuments_ShouldReturnNull()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var result = provider.GetMainDocument();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddDocument Edge Cases

    [Fact]
    public void AddDocument_WithNullDocument_ShouldThrowArgumentNullException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act & Assert
        var action = () => provider.AddDocument(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddDocument_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.Delete(new MockDateTimeProvider());
        var document = new Document("11144477735", EDocumentType.CPF);

        // Act & Assert
        var action = () => provider.AddDocument(document);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot add document to deleted provider");
    }

    #endregion

    #region RemoveDocument Edge Cases

    [Fact]
    public void RemoveDocument_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var document = new Document("11144477735", EDocumentType.CPF);
        provider.AddDocument(document);
        provider.Delete(new MockDateTimeProvider());

        // Act & Assert
        var action = () => provider.RemoveDocument(EDocumentType.CPF);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot remove document from deleted provider");
    }

    #endregion

    #region AddQualification Edge Cases

    [Fact]
    public void AddQualification_WithNullQualification_ShouldThrowArgumentNullException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act & Assert
        var action = () => provider.AddQualification(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddQualification_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.Delete(new MockDateTimeProvider());
        var qualification = new Qualification("Test Qualification");

        // Act & Assert
        var action = () => provider.AddQualification(qualification);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot add qualification to deleted provider");
    }

    #endregion

    #region RemoveQualification Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void RemoveQualification_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument - This is intentional for testing
        var action = () => provider.RemoveQualification(invalidName);
#pragma warning restore CS8604
        action.Should().Throw<ArgumentException>()
            .WithMessage("Qualification name cannot be empty*");
    }

    [Fact]
    public void RemoveQualification_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var qualification = new Qualification("Test Qualification");
        provider.AddQualification(qualification);
        provider.Delete(new MockDateTimeProvider());

        // Act & Assert
        var action = () => provider.RemoveQualification("Test Qualification");
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot remove qualification from deleted provider");
    }

    [Fact]
    public void RemoveQualification_WithNonExistingQualification_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act & Assert
        var action = () => provider.RemoveQualification("Non Existing Qualification");
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Qualification 'Non Existing Qualification' not found");
    }

    #endregion

    #region UpdateVerificationStatus Edge Cases

    [Fact]
    public void UpdateVerificationStatus_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.Delete(new MockDateTimeProvider());

        // Act & Assert
        var action = () => provider.UpdateVerificationStatus(EVerificationStatus.Verified);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot update verification status of deleted provider");
    }

    [Fact]
    public void UpdateVerificationStatus_WithSkipMarkAsUpdated_ShouldNotCallMarkAsUpdated()
    {
        // Arrange
        var provider = CreateValidProvider();
        var originalUpdatedAt = provider.UpdatedAt;

        // Wait a bit to ensure time difference would be detectable
        System.Threading.Thread.Sleep(10);

        // Act
        provider.UpdateVerificationStatus(EVerificationStatus.Verified, "admin@test.com", skipMarkAsUpdated: true);

        // Assert
        provider.VerificationStatus.Should().Be(EVerificationStatus.Verified);
        // UpdatedAt should NOT have changed since we skipped MarkAsUpdated
        provider.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    #endregion

    #region Status Reason Fields Tests

    [Fact]
    public void UpdateStatus_FromSuspendedToActive_ShouldClearSuspensionReason()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();
        provider.Activate();
        provider.Suspend("Violation", "admin@test.com");
        provider.SuspensionReason.Should().Be("Violation");

        // Act
        provider.Reactivate("admin@test.com");

        // Assert
        provider.Status.Should().Be(EProviderStatus.Active);
        provider.SuspensionReason.Should().BeNull();
    }

    [Fact]
    public void UpdateStatus_FromRejectedToPendingBasicInfo_ShouldClearRejectionReason()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();
        provider.Reject("Invalid documents", "admin@test.com");
        provider.RejectionReason.Should().Be("Invalid documents");

        // Act
        provider.UpdateStatus(EProviderStatus.PendingBasicInfo, "admin@test.com");

        // Assert
        provider.Status.Should().Be(EProviderStatus.PendingBasicInfo);
        provider.RejectionReason.Should().BeNull();
    }

    #endregion

    #region Provider Creation Validation Tests

    [Fact]
    public void Constructor_WithNameLessThan2Characters_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "A"; // Only 1 character
        var type = EProviderType.Individual;
        var businessProfile = CreateValidBusinessProfile();

        // Act & Assert
        var action = () => new Provider(userId, name, type, businessProfile);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Name must be at least 2 characters long");
    }

    [Fact]
    public void Constructor_WithNameExceeding100Characters_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = new string('A', 101); // 101 characters
        var type = EProviderType.Individual;
        var businessProfile = CreateValidBusinessProfile();

        // Act & Assert
        var action = () => new Provider(userId, name, type, businessProfile);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Name cannot exceed 100 characters");
    }

    [Fact]
    public void Constructor_WithNameExactly2Characters_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "AB"; // Exactly 2 characters
        var type = EProviderType.Individual;
        var businessProfile = CreateValidBusinessProfile();

        // Act
        var provider = new Provider(userId, name, type, businessProfile);

        // Assert
        provider.Name.Should().Be("AB");
    }

    [Fact]
    public void Constructor_WithNameExactly100Characters_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = new string('A', 100); // Exactly 100 characters
        var type = EProviderType.Individual;
        var businessProfile = CreateValidBusinessProfile();

        // Act
        var provider = new Provider(userId, name, type, businessProfile);

        // Assert
        provider.Name.Length.Should().Be(100);
    }

    #endregion

    #region Suspend and Reject Edge Cases

    [Fact]
    public void Suspend_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.Delete(new MockDateTimeProvider());

        // Act & Assert
        var action = () => provider.Suspend("Reason", "admin@test.com");
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot suspend deleted provider");
    }

    [Fact]
    public void Reject_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.Delete(new MockDateTimeProvider());

        // Act & Assert
        var action = () => provider.Reject("Reason", "admin@test.com");
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot reject deleted provider");
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_ShouldNotChange()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();
        provider.Reject("Initial reason", "admin@test.com");
        var previousUpdateTime = provider.UpdatedAt;

        // Act
        provider.Reject("Another reason", "admin@test.com");

        // Assert
        provider.Status.Should().Be(EProviderStatus.Rejected);
        provider.UpdatedAt.Should().Be(previousUpdateTime);
    }

    #endregion

    #region RemoveService Edge Cases

    [Fact]
    public void RemoveService_FromDeletedProvider_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceId = Guid.NewGuid();
        provider.AddService(serviceId);
        provider.Delete(new MockDateTimeProvider());

        // Act & Assert
        var action = () => provider.RemoveService(serviceId);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot remove services from deleted provider");
    }

    #endregion
}
