using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.Entities;

/// <summary>
/// Testes completos para Provider Entity incluindo casos padrão e edge cases.
/// Consolidado de ProviderTests e ProviderEdgeCasesTests para evitar duplicação.
/// Coverage: Construção, validações, transições de estado, eventos de domínio, serviços, qualificações e documentos.
/// </summary>
[Trait("Category", "Unit")]
public class ProviderTests
{
    // Cria um mock do provedor de data/hora
    private static FakeTimeProvider CreateMockDateTimeProvider(DateTime? fixedDate = null)
    {
        return fixedDate.HasValue
            ? new FakeTimeProvider(new DateTimeOffset(fixedDate.Value, TimeSpan.Zero))
            : new FakeTimeProvider(DateTimeOffset.UtcNow);
    }

    private static BusinessProfile CreateValidBusinessProfile()
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
            email: "test@provider.com",
            phoneNumber: "+55 11 99999-9999",
            website: "https://www.provider.com");

        return new BusinessProfile(
            legalName: "Provider Test LTDA",
            contactInfo: contactInfo,
            primaryAddress: address);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "John Provider";
        var type = EProviderType.Individual;
        var businessProfile = CreateValidBusinessProfile();

        // Act
        var provider = new Provider(userId, name, type, businessProfile);

        // Assert
        provider.Id.Should().NotBeNull();
        provider.Id.Value.Should().NotBe(Guid.Empty);
        provider.UserId.Should().Be(userId);
        provider.Name.Should().Be(name);
        provider.Type.Should().Be(type);
        provider.BusinessProfile.Should().Be(businessProfile);
        provider.VerificationStatus.Should().Be(EVerificationStatus.Pending);
        provider.IsDeleted.Should().BeFalse();
        provider.DeletedAt.Should().BeNull();
        provider.Documents.Should().BeEmpty();
        provider.Qualifications.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldRaiseProviderRegisteredDomainEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "John Provider";
        var type = EProviderType.Individual;
        var businessProfile = CreateValidBusinessProfile();

        // Act
        var provider = new Provider(userId, name, type, businessProfile);

        // Assert
        provider.DomainEvents.Should().HaveCount(1);
        var domainEvent = provider.DomainEvents.Single();
        domainEvent.Should().BeOfType<ProviderRegisteredDomainEvent>();

        var registeredEvent = (ProviderRegisteredDomainEvent)domainEvent;
        registeredEvent.AggregateId.Should().Be(provider.Id.Value);
        registeredEvent.Version.Should().Be(1);
        registeredEvent.UserId.Should().Be(userId);
        registeredEvent.Name.Should().Be(name);
        registeredEvent.Type.Should().Be(type);
        registeredEvent.Email.Should().Be(businessProfile.ContactInfo.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ShouldThrowProviderDomainException(string? invalidName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var type = EProviderType.Individual;
        var businessProfile = CreateValidBusinessProfile();

        // Act & Assert - Testing that null name is rejected (intentional null)
#pragma warning disable CS8604 // Possible null reference argument - This is intentional for testing
        var action = () => new Provider(userId, invalidName, type, businessProfile);
#pragma warning restore CS8604
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Name cannot be empty");
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowProviderDomainException()
    {
        // Arrange
        var userId = Guid.Empty;
        var name = "John Provider";
        var type = EProviderType.Individual;
        var businessProfile = CreateValidBusinessProfile();

        // Act & Assert
        var action = () => new Provider(userId, name, type, businessProfile);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("UserId cannot be empty");
    }

    [Fact]
    public void Constructor_WithNullBusinessProfile_ShouldThrowArgumentNullException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "John Provider";
        var type = EProviderType.Individual;

        // Act & Assert
        var action = () => new Provider(userId, name, type, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateProfile_WithValidParameters_ShouldUpdateProvider()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newName = "Updated Provider Name";
        var newBusinessProfile = CreateValidBusinessProfile();
        var updatedBy = "admin@test.com";

        // Act
        provider.UpdateProfile(newName, newBusinessProfile, updatedBy);

        // Assert
        provider.Name.Should().Be(newName);
        provider.BusinessProfile.Should().Be(newBusinessProfile);

        provider.DomainEvents.Should().HaveCount(2); // Registered + Updated
        var updateEvent = provider.DomainEvents.Last();
        updateEvent.Should().BeOfType<ProviderProfileUpdatedDomainEvent>();

        var profileUpdatedEvent = (ProviderProfileUpdatedDomainEvent)updateEvent;
        profileUpdatedEvent.Name.Should().Be(newName);
        profileUpdatedEvent.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void UpdateProfile_WhenDeleted_ShouldThrowProviderDomainException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var dateTimeProvider = CreateMockDateTimeProvider();
        provider.Delete(dateTimeProvider);

        var newName = "Updated Name";
        var newBusinessProfile = CreateValidBusinessProfile();

        // Act & Assert
        var action = () => provider.UpdateProfile(newName, newBusinessProfile);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot update deleted provider");
    }

    [Fact]
    public void AddDocument_WithValidDocument_ShouldAddDocument()
    {
        // Arrange
        var provider = CreateValidProvider();
        var document = new Document("11144477735", EDocumentType.CPF);

        // Act
        provider.AddDocument(document);

        // Assert
        provider.Documents.Should().HaveCount(1);
        provider.Documents.Should().Contain(document);

        var addEvent = provider.DomainEvents.Last();
        addEvent.Should().BeOfType<ProviderDocumentAddedDomainEvent>();

        var documentAddedEvent = (ProviderDocumentAddedDomainEvent)addEvent;
        documentAddedEvent.DocumentType.Should().Be(EDocumentType.CPF);
        documentAddedEvent.DocumentNumber.Should().Be("11144477735");
    }

    [Fact]
    public void AddDocument_WithDuplicateDocumentType_ShouldThrowProviderDomainException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var document1 = new Document("11144477735", EDocumentType.CPF);
        var document2 = new Document("12345678909", EDocumentType.CPF);

        provider.AddDocument(document1);

        // Act & Assert
        var action = () => provider.AddDocument(document2);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Document of type CPF already exists");
    }

    [Fact]
    public void RemoveDocument_WithExistingDocument_ShouldRemoveDocument()
    {
        // Arrange
        var provider = CreateValidProvider();
        var document = new Document("11144477735", EDocumentType.CPF);
        provider.AddDocument(document);

        // Act
        provider.RemoveDocument(EDocumentType.CPF);

        // Assert
        provider.Documents.Should().BeEmpty();

        var removeEvent = provider.DomainEvents.Last();
        removeEvent.Should().BeOfType<ProviderDocumentRemovedDomainEvent>();
    }

    [Fact]
    public void RemoveDocument_WithNonExistentDocument_ShouldThrowProviderDomainException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act & Assert
        var action = () => provider.RemoveDocument(EDocumentType.CPF);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("Document of type CPF not found");
    }

    [Fact]
    public void AddQualification_WithValidQualification_ShouldAddQualification()
    {
        // Arrange
        var provider = CreateValidProvider();
        var qualification = new Qualification(
            name: "Certified Professional",
            description: "Professional certification",
            issuingOrganization: "Certification Board");

        // Act
        provider.AddQualification(qualification);

        // Assert
        provider.Qualifications.Should().HaveCount(1);
        provider.Qualifications.Should().Contain(qualification);

        var addEvent = provider.DomainEvents.Last();
        addEvent.Should().BeOfType<ProviderQualificationAddedDomainEvent>();

        var qualificationAddedEvent = (ProviderQualificationAddedDomainEvent)addEvent;
        qualificationAddedEvent.QualificationName.Should().Be("Certified Professional");
        qualificationAddedEvent.IssuingOrganization.Should().Be("Certification Board");
    }

    [Fact]
    public void AddQualification_WithDuplicateName_ShouldThrowProviderDomainException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var qualification1 = new Qualification("Test Qualification", "First description");
        var qualification2 = new Qualification("Test Qualification", "Second description");

        provider.AddQualification(qualification1);

        // Act & Assert
        var action = () => provider.AddQualification(qualification2);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("A qualification with the name 'Test Qualification' already exists");
    }

    [Fact]
    public void AddQualification_WithDuplicateNameDifferentCase_ShouldThrowProviderDomainException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var qualification1 = new Qualification("Test Qualification", "First description");
        var qualification2 = new Qualification("TEST QUALIFICATION", "Second description");

        provider.AddQualification(qualification1);

        // Act & Assert
        var action = () => provider.AddQualification(qualification2);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("A qualification with the name 'TEST QUALIFICATION' already exists");
    }

    [Fact]
    public void RemoveQualification_WithExistingQualification_ShouldRemoveQualification()
    {
        // Arrange
        var provider = CreateValidProvider();
        var qualification = new Qualification("Test Qualification");
        provider.AddQualification(qualification);

        // Act
        provider.RemoveQualification("Test Qualification");

        // Assert
        provider.Qualifications.Should().BeEmpty();

        var removeEvent = provider.DomainEvents.Last();
        removeEvent.Should().BeOfType<ProviderQualificationRemovedDomainEvent>();
    }

    [Fact]
    public void UpdateVerificationStatus_WithNewStatus_ShouldUpdateStatus()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newStatus = EVerificationStatus.Verified;
        var updatedBy = "admin@test.com";

        // Act
        provider.UpdateVerificationStatus(newStatus, updatedBy);

        // Assert
        provider.VerificationStatus.Should().Be(newStatus);

        var updateEvent = provider.DomainEvents.Last();
        updateEvent.Should().BeOfType<ProviderVerificationStatusUpdatedDomainEvent>();

        var statusUpdatedEvent = (ProviderVerificationStatusUpdatedDomainEvent)updateEvent;
        statusUpdatedEvent.PreviousStatus.Should().Be(EVerificationStatus.Pending);
        statusUpdatedEvent.NewStatus.Should().Be(newStatus);
        statusUpdatedEvent.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void Delete_ShouldMarkProviderAsDeleted()
    {
        // Arrange
        var provider = CreateValidProvider();
        var fixedDate = DateTime.UtcNow;
        var dateTimeProvider = CreateMockDateTimeProvider(fixedDate);

        // Act
        provider.Delete(dateTimeProvider);

        // Assert
        provider.IsDeleted.Should().BeTrue();
        provider.DeletedAt.Should().Be(fixedDate);

        var deleteEvent = provider.DomainEvents.Last();
        deleteEvent.Should().BeOfType<ProviderDeletedDomainEvent>();

        var deletedEvent = (ProviderDeletedDomainEvent)deleteEvent;
        deletedEvent.Name.Should().Be(provider.Name);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldNotChangeState()
    {
        // Arrange
        var provider = CreateValidProvider();
        var dateTimeProvider = CreateMockDateTimeProvider();
        provider.Delete(dateTimeProvider);

        var originalDeletedAt = provider.DeletedAt;
        var originalEventCount = provider.DomainEvents.Count;

        // Act
        provider.Delete(dateTimeProvider);

        // Assert
        provider.IsDeleted.Should().BeTrue();
        provider.DeletedAt.Should().Be(originalDeletedAt);
        provider.DomainEvents.Should().HaveCount(originalEventCount); // No new events
    }

    private static Provider CreateValidProvider()
    {
        var userId = Guid.NewGuid();
        var name = "Test Provider";
        var type = EProviderType.Individual;
        var businessProfile = CreateValidBusinessProfile();

        return new Provider(userId, name, type, businessProfile);
    }

    #region Status Transition Tests

    [Fact]
    public void Constructor_ShouldSetInitialStatusToPendingBasicInfo()
    {
        // Arrange & Act
        var provider = CreateValidProvider();

        // Assert
        provider.Status.Should().Be(EProviderStatus.PendingBasicInfo);
    }

    [Fact]
    public void CompleteBasicInfo_WhenInPendingBasicInfo_ShouldTransitionToPendingDocumentVerification()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.ClearDomainEvents(); // Clear registration event

        // Act
        provider.CompleteBasicInfo("admin@test.com");

        // Assert
        provider.Status.Should().Be(EProviderStatus.PendingDocumentVerification);
        provider.DomainEvents.Should().ContainSingle(e => e is ProviderAwaitingVerificationDomainEvent);
    }

    [Fact]
    public void CompleteBasicInfo_WhenNotInPendingBasicInfo_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo(); // Transition to PendingDocumentVerification

        // Act
        var act = () => provider.CompleteBasicInfo();

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot complete basic info when not in PendingBasicInfo status");
    }

    [Fact]
    public void Activate_WhenInPendingDocumentVerification_ShouldTransitionToActive()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();
        provider.ClearDomainEvents();

        // Act
        provider.Activate("admin@test.com");

        // Assert
        provider.Status.Should().Be(EProviderStatus.Active);
        provider.VerificationStatus.Should().Be(EVerificationStatus.Verified);
        provider.DomainEvents.Should().ContainSingle(e => e is ProviderActivatedDomainEvent);
    }

    [Fact]
    public void Activate_WhenNotInPendingDocumentVerification_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var act = () => provider.Activate();

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Can only activate providers in PendingDocumentVerification status");
    }

    [Fact]
    public void Suspend_WhenActive_ShouldTransitionToSuspended()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();
        provider.Activate();

        // Act
        provider.Suspend("Violation of terms of service", "admin@test.com");

        // Assert
        provider.Status.Should().Be(EProviderStatus.Suspended);
        provider.VerificationStatus.Should().Be(EVerificationStatus.Suspended);
        provider.SuspensionReason.Should().Be("Violation of terms of service");
    }

    [Fact]
    public void Suspend_WithoutReason_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();
        provider.Activate();

        // Act
        var act = () => provider.Suspend("", "admin@test.com");

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Suspension reason is required");
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ShouldNotChange()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();
        provider.Activate();
        provider.Suspend("Initial reason", "admin@test.com");
        var previousUpdateTime = provider.UpdatedAt;

        // Act
        provider.Suspend("Another reason", "admin@test.com");

        // Assert
        provider.Status.Should().Be(EProviderStatus.Suspended);
        provider.UpdatedAt.Should().Be(previousUpdateTime);
    }

    [Fact]
    public void Reject_WhenInPendingDocumentVerification_ShouldTransitionToRejected()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();

        // Act
        provider.Reject("Invalid documentation provided", "admin@test.com");

        // Assert
        provider.Status.Should().Be(EProviderStatus.Rejected);
        provider.VerificationStatus.Should().Be(EVerificationStatus.Rejected);
        provider.RejectionReason.Should().Be("Invalid documentation provided");
    }

    [Fact]
    public void Reject_WithoutReason_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();

        // Act
        var act = () => provider.Reject("", "admin@test.com");

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Rejection reason is required");
    }

    [Fact]
    public void UpdateStatus_WithInvalidTransition_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act - Try to go directly from PendingBasicInfo to Active (not allowed)
        var act = () => provider.UpdateStatus(EProviderStatus.Active);

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Invalid status transition from PendingBasicInfo to Active");
    }

    [Fact]
    public void UpdateStatus_WhenProviderIsDeleted_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var dateTimeProvider = CreateMockDateTimeProvider();
        provider.Delete(dateTimeProvider);

        // Act
        var act = () => provider.UpdateStatus(EProviderStatus.Active);

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot update status of deleted provider");
    }

    [Theory]
    [InlineData(EProviderStatus.PendingBasicInfo, EProviderStatus.PendingDocumentVerification, true)]
    [InlineData(EProviderStatus.PendingBasicInfo, EProviderStatus.Rejected, true)]
    [InlineData(EProviderStatus.PendingDocumentVerification, EProviderStatus.Active, true)]
    [InlineData(EProviderStatus.PendingDocumentVerification, EProviderStatus.Rejected, true)]
    [InlineData(EProviderStatus.PendingDocumentVerification, EProviderStatus.PendingBasicInfo, true)]
    [InlineData(EProviderStatus.Active, EProviderStatus.Suspended, true)]
    [InlineData(EProviderStatus.Suspended, EProviderStatus.Active, true)]
    [InlineData(EProviderStatus.Suspended, EProviderStatus.Rejected, true)]
    [InlineData(EProviderStatus.Rejected, EProviderStatus.PendingBasicInfo, true)]
    [InlineData(EProviderStatus.PendingBasicInfo, EProviderStatus.Active, false)]
    [InlineData(EProviderStatus.PendingBasicInfo, EProviderStatus.Suspended, false)]
    [InlineData(EProviderStatus.Active, EProviderStatus.PendingBasicInfo, false)]
    public void StatusTransitions_ShouldFollowDefinedRules(EProviderStatus from, EProviderStatus to, bool shouldSucceed)
    {
        // Arrange
        var provider = CreateValidProvider();

        // Set the provider to the "from" status
        // This is a bit hacky but necessary for testing
        if (from == EProviderStatus.PendingDocumentVerification)
        {
            provider.CompleteBasicInfo();
        }
        else if (from == EProviderStatus.Active)
        {
            provider.CompleteBasicInfo();
            provider.Activate();
        }
        else if (from == EProviderStatus.Suspended)
        {
            provider.CompleteBasicInfo();
            provider.Activate();
            provider.Suspend("Test suspension", "admin@test.com");
        }
        else if (from == EProviderStatus.Rejected)
        {
            provider.CompleteBasicInfo();
            provider.Reject("Test rejection", "admin@test.com");
        }

        // Act & Assert
        if (shouldSucceed)
        {
            // Use appropriate method for transitions that require reasons
            if (to == EProviderStatus.Suspended)
            {
                var act = () => provider.Suspend("Test suspension reason", "admin@test.com");
                act.Should().NotThrow();
            }
            else if (to == EProviderStatus.Rejected)
            {
                var act = () => provider.Reject("Test rejection reason", "admin@test.com");
                act.Should().NotThrow();
            }
            else if (to == EProviderStatus.Active && from == EProviderStatus.Suspended)
            {
                var act = () => provider.Reactivate("admin@test.com");
                act.Should().NotThrow();
            }
            else
            {
                var act = () => provider.UpdateStatus(to);
                act.Should().NotThrow();
            }

            provider.Status.Should().Be(to);
        }
        else
        {
            var act = () => provider.UpdateStatus(to);
            act.Should().Throw<ProviderDomainException>()
                .WithMessage($"Invalid status transition from {from} to {to}");
        }
    }

    [Fact]
    public void Reactivate_WhenSuspended_ShouldTransitionToActive()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();
        provider.Activate();
        provider.Suspend("Policy violation", "admin@test.com");

        // Act
        provider.Reactivate("admin@test.com");

        // Assert
        provider.Status.Should().Be(EProviderStatus.Active);
        provider.VerificationStatus.Should().Be(EVerificationStatus.Verified);
    }

    [Theory]
    [InlineData(EProviderStatus.PendingBasicInfo)]
    [InlineData(EProviderStatus.PendingDocumentVerification)]
    [InlineData(EProviderStatus.Active)]
    [InlineData(EProviderStatus.Rejected)]
    public void Reactivate_WhenNotInSuspendedStatus_ShouldThrowException(EProviderStatus currentStatus)
    {
        // Arrange
        var provider = CreateValidProvider();

        // Set provider to the specified status
        if (currentStatus == EProviderStatus.PendingDocumentVerification)
        {
            provider.CompleteBasicInfo();
        }
        else if (currentStatus == EProviderStatus.Active)
        {
            provider.CompleteBasicInfo();
            provider.Activate();
        }
        else if (currentStatus == EProviderStatus.Rejected)
        {
            provider.CompleteBasicInfo();
            provider.Reject("Invalid documents", "admin@test.com");
        }
        // PendingBasicInfo is the default status, no action needed

        // Act
        var act = () => provider.Reactivate("admin@test.com");

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Can only reactivate providers in Suspended status");
    }

    [Fact]
    public void RequireBasicInfoCorrection_WhenPendingDocumentVerification_ShouldTransitionToPendingBasicInfo()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();
        var reason = "Missing required information in business profile";

        // Act
        provider.RequireBasicInfoCorrection(reason, "verifier@test.com");

        // Assert
        provider.Status.Should().Be(EProviderStatus.PendingBasicInfo);
        provider.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(EProviderStatus.PendingBasicInfo)]
    [InlineData(EProviderStatus.Active)]
    [InlineData(EProviderStatus.Suspended)]
    [InlineData(EProviderStatus.Rejected)]
    public void RequireBasicInfoCorrection_WhenNotPendingDocumentVerification_ShouldThrowException(EProviderStatus currentStatus)
    {
        // Arrange
        var provider = CreateValidProvider();

        // Set provider to the specified status
        if (currentStatus == EProviderStatus.Active)
        {
            provider.CompleteBasicInfo();
            provider.Activate();
        }
        else if (currentStatus == EProviderStatus.Suspended)
        {
            provider.CompleteBasicInfo();
            provider.Activate();
            provider.Suspend("Policy violation", "admin@test.com");
        }
        else if (currentStatus == EProviderStatus.Rejected)
        {
            provider.CompleteBasicInfo();
            provider.Reject("Invalid documents", "admin@test.com");
        }
        // PendingBasicInfo is the default status, no action needed

        // Act
        var act = () => provider.RequireBasicInfoCorrection("Some reason", "verifier@test.com");

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Can only require basic info correction during document verification");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequireBasicInfoCorrection_WithInvalidReason_ShouldThrowException(string? invalidReason)
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.CompleteBasicInfo();

        // Act
        var act = () => provider.RequireBasicInfoCorrection(invalidReason!, "verifier@test.com");

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Correction reason is required");
    }

    #endregion

    #region Tier Tests

    [Fact]
    public void PromoteTier_ShouldUpdateTierAndAddEvent()
    {
        // Arrange
        var provider = CreateValidProvider();
        var initialTier = provider.Tier;
        var newTier = EProviderTier.Gold;
        var updatedBy = "stripe-webhook";

        // Act
        provider.PromoteTier(newTier, updatedBy);

        // Assert
        provider.Tier.Should().Be(newTier);
        
        var tierEvent = provider.DomainEvents.Last();
        tierEvent.Should().BeOfType<ProviderTierUpdatedDomainEvent>();

        var updatedEvent = (ProviderTierUpdatedDomainEvent)tierEvent;
        updatedEvent.PreviousTier.Should().Be(initialTier);
        updatedEvent.NewTier.Should().Be(newTier);
        updatedEvent.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void PromoteTier_WithSameTier_ShouldDoNothing()
    {
        // Arrange
        var provider = CreateValidProvider();
        var initialTier = provider.Tier;
        var initialEventsCount = provider.DomainEvents.Count;

        // Act
        provider.PromoteTier(initialTier, "system");

        // Assert
        provider.Tier.Should().Be(initialTier);
        provider.DomainEvents.Should().HaveCount(initialEventsCount);
    }

    [Fact]
    public void PromoteTier_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var dateTimeProvider = CreateMockDateTimeProvider();
        provider.Delete(dateTimeProvider);

        // Act
        var act = () => provider.PromoteTier(EProviderTier.Platinum, "system");

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot update tier of deleted provider");
    }

    #endregion

    #region ProviderServices Tests

    [Fact]
    public void AddService_WithValidServiceId_ShouldAddService()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceId = Guid.NewGuid();
        var serviceName = "New Service";

        // Act
        provider.AddService(serviceId, serviceName);

        // Assert
        provider.Services.Should().HaveCount(1);
        var service = provider.Services.Single();
        service.ServiceId.Should().Be(serviceId);
        service.ServiceName.Should().Be(serviceName);
        service.ProviderId.Should().Be(provider.Id);
        provider.OffersService(serviceId).Should().BeTrue();
        provider.GetServiceIds().Should().ContainSingle().Which.Should().Be(serviceId);

        var addEvent = provider.DomainEvents.Last();
        addEvent.Should().BeOfType<ProviderServiceAddedDomainEvent>();

        var serviceAddedEvent = (ProviderServiceAddedDomainEvent)addEvent;
        serviceAddedEvent.ServiceId.Should().Be(serviceId);
    }

    [Fact]
    public void AddService_WithMultipleServices_ShouldAddAll()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();
        var serviceId3 = Guid.NewGuid();

        // Act
        provider.AddService(serviceId1, "Service 1");
        provider.AddService(serviceId2, "Service 2");
        provider.AddService(serviceId3, "Service 3");

        // Assert
        provider.Services.Should().HaveCount(3);
        var serviceIds = provider.GetServiceIds();
        serviceIds.Should().Contain(serviceId1);
        serviceIds.Should().Contain(serviceId2);
        serviceIds.Should().Contain(serviceId3);
    }

    [Fact]
    public void AddService_WithEmptyGuid_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var act = () => provider.AddService(Guid.Empty, "Service Name");

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("ServiceId cannot be empty");
    }

    [Fact]
    public void AddService_WithDuplicateService_ShouldThrowProviderDomainException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceId = Guid.NewGuid();
        var serviceName = "Test Service";
        provider.AddService(serviceId, serviceName);

        // Act & Assert
        var action = () => provider.AddService(serviceId, serviceName);
        action.Should().Throw<ProviderDomainException>()
            .WithMessage($"Service {serviceId} is already offered by this provider");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void AddService_WithInvalidName_ShouldThrowProviderDomainException(string? invalidName)
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceId = Guid.NewGuid();

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument
        var action = () => provider.AddService(serviceId, invalidName);
#pragma warning restore CS8604
        
        action.Should().Throw<ProviderDomainException>()
            .WithMessage("ServiceName cannot be empty");
    }

    [Fact]
    public void AddService_ToDeletedProvider_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var dateTimeProvider = CreateMockDateTimeProvider();
        provider.Delete(dateTimeProvider);
        var serviceId = Guid.NewGuid();

        // Act
        var act = () => provider.AddService(serviceId, "Service Name");

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("Cannot add services to deleted provider");
    }

    [Fact]
    public void RemoveService_WithExistingService_ShouldRemoveService()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceId = Guid.NewGuid();
        provider.AddService(serviceId, "Service Name");

        // Act
        provider.RemoveService(serviceId);

        // Assert
        provider.Services.Should().BeEmpty();
        provider.OffersService(serviceId).Should().BeFalse();
        provider.GetServiceIds().Should().BeEmpty();
    }

    [Fact]
    public void RemoveService_WithNonExistingService_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceId = Guid.NewGuid();

        // Act
        var act = () => provider.RemoveService(serviceId);

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage($"Service {serviceId} is not offered by this provider");
    }

    [Fact]
    public void RemoveService_WithEmptyGuid_ShouldThrowException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var act = () => provider.RemoveService(Guid.Empty);

        // Assert
        act.Should().Throw<ProviderDomainException>()
            .WithMessage("ServiceId cannot be empty");
    }

    [Fact]
    public void OffersService_WithExistingService_ShouldReturnTrue()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceId = Guid.NewGuid();
        provider.AddService(serviceId, "Service Name");

        // Act
        var result = provider.OffersService(serviceId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OffersService_WithNonExistingService_ShouldReturnFalse()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceId = Guid.NewGuid();

        // Act
        var result = provider.OffersService(serviceId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetServiceIds_WithNoServices_ShouldReturnEmptyArray()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var serviceIds = provider.GetServiceIds();

        // Assert
        serviceIds.Should().BeEmpty();
        serviceIds.Should().BeOfType<Guid[]>();
    }

    [Fact]
    public void GetServiceIds_WithMultipleServices_ShouldReturnAllIds()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();
        var serviceId3 = Guid.NewGuid();
        provider.AddService(serviceId1, "Service 1");
        provider.AddService(serviceId2, "Service 2");
        provider.AddService(serviceId3, "Service 3");

        // Act
        var serviceIds = provider.GetServiceIds();

        // Assert
        serviceIds.Should().HaveCount(3);
        serviceIds.Should().Contain(new[] { serviceId1, serviceId2, serviceId3 });
    }

    #endregion
}

