using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.Events;

[Trait("Category", "Unit")]
public class ProviderDomainEventsTests
{
    [Fact]
    public void ProviderActivatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 5;
        var userId = Guid.NewGuid();
        var name = "Healthcare Provider";
        var activatedBy = "admin@example.com";

        // Act
        var domainEvent = new ProviderActivatedDomainEvent(
            aggregateId,
            version,
            userId,
            name,
            activatedBy);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.Name.Should().Be(name);
        domainEvent.ActivatedBy.Should().Be(activatedBy);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderActivatedDomainEvent_WithNullActivatedBy_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 5;
        var userId = Guid.NewGuid();
        var name = "Healthcare Provider";

        // Act
        var domainEvent = new ProviderActivatedDomainEvent(
            aggregateId,
            version,
            userId,
            name,
            null);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.Name.Should().Be(name);
        domainEvent.ActivatedBy.Should().BeNull();
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderRegisteredDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var userId = Guid.NewGuid();
        var name = "New Healthcare Provider";
        var type = EProviderType.Individual;
        var email = "provider@example.com";

        // Act
        var domainEvent = new ProviderRegisteredDomainEvent(
            aggregateId,
            version,
            userId,
            name,
            type,
            email);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.Name.Should().Be(name);
        domainEvent.Type.Should().Be(type);
        domainEvent.Email.Should().Be(email);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderAwaitingVerificationDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 3;
        var userId = Guid.NewGuid();
        var name = "Provider Name";
        var updatedBy = "admin@example.com";

        // Act
        var domainEvent = new ProviderAwaitingVerificationDomainEvent(
            aggregateId,
            version,
            userId,
            name,
            updatedBy);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.Name.Should().Be(name);
        domainEvent.UpdatedBy.Should().Be(updatedBy);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderBasicInfoCorrectionRequiredDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 4;
        var userId = Guid.NewGuid();
        var name = "Provider Name";
        var reason = "Invalid business license number";
        var requestedBy = "verifier@example.com";

        // Act
        var domainEvent = new ProviderBasicInfoCorrectionRequiredDomainEvent(
            aggregateId,
            version,
            userId,
            name,
            reason,
            requestedBy);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.Name.Should().Be(name);
        domainEvent.Reason.Should().Be(reason);
        domainEvent.RequestedBy.Should().Be(requestedBy);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderDeletedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 10;
        var name = "Provider to Delete";
        var deletedBy = "admin@example.com";

        // Act
        var domainEvent = new ProviderDeletedDomainEvent(
            aggregateId,
            version,
            name,
            deletedBy);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.Name.Should().Be(name);
        domainEvent.DeletedBy.Should().Be(deletedBy);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderDocumentAddedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 6;
        var documentType = EDocumentType.CPF;
        var documentNumber = "123.456.789-00";

        // Act
        var domainEvent = new ProviderDocumentAddedDomainEvent(
            aggregateId,
            version,
            documentType,
            documentNumber);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.DocumentType.Should().Be(documentType);
        domainEvent.DocumentNumber.Should().Be(documentNumber);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderDocumentRemovedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 7;
        var documentType = EDocumentType.CNPJ;
        var documentNumber = "12.345.678/0001-90";

        // Act
        var domainEvent = new ProviderDocumentRemovedDomainEvent(
            aggregateId,
            version,
            documentType,
            documentNumber);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.DocumentType.Should().Be(documentType);
        domainEvent.DocumentNumber.Should().Be(documentNumber);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderProfileUpdatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 8;
        var name = "Updated Provider Name";
        var email = "updated@example.com";
        var updatedBy = "provider@example.com";
        var updatedFields = new[] { "Name", "Email", "ContactInfo" };

        // Act
        var domainEvent = new ProviderProfileUpdatedDomainEvent(
            aggregateId,
            version,
            name,
            email,
            updatedBy,
            updatedFields);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.Name.Should().Be(name);
        domainEvent.Email.Should().Be(email);
        domainEvent.UpdatedBy.Should().Be(updatedBy);
        domainEvent.UpdatedFields.Should().BeEquivalentTo(updatedFields);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderQualificationAddedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 9;
        var qualificationName = "Medical License";
        var issuingOrganization = "State Medical Board";

        // Act
        var domainEvent = new ProviderQualificationAddedDomainEvent(
            aggregateId,
            version,
            qualificationName,
            issuingOrganization);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.QualificationName.Should().Be(qualificationName);
        domainEvent.IssuingOrganization.Should().Be(issuingOrganization);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderQualificationRemovedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 10;
        var qualificationName = "Expired License";
        var issuingOrganization = "State Medical Board";

        // Act
        var domainEvent = new ProviderQualificationRemovedDomainEvent(
            aggregateId,
            version,
            qualificationName,
            issuingOrganization);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.QualificationName.Should().Be(qualificationName);
        domainEvent.IssuingOrganization.Should().Be(issuingOrganization);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderServiceAddedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 11;
        var serviceId = Guid.NewGuid();

        // Act
        var domainEvent = new ProviderServiceAddedDomainEvent(
            aggregateId,
            version,
            serviceId);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.ServiceId.Should().Be(serviceId);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderServiceRemovedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 12;
        var serviceId = Guid.NewGuid();

        // Act
        var domainEvent = new ProviderServiceRemovedDomainEvent(
            aggregateId,
            version,
            serviceId);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.ServiceId.Should().Be(serviceId);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderVerificationStatusUpdatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 13;
        var previousStatus = EVerificationStatus.Pending;
        var newStatus = EVerificationStatus.Verified;
        var updatedBy = "verifier@example.com";

        // Act
        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            aggregateId,
            version,
            previousStatus,
            newStatus,
            updatedBy);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.PreviousStatus.Should().Be(previousStatus);
        domainEvent.NewStatus.Should().Be(newStatus);
        domainEvent.UpdatedBy.Should().Be(updatedBy);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }
}
