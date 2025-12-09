using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Events;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Domain.Events;

[Trait("Category", "Unit")]
public class DocumentDomainEventsTests
{
    [Fact]
    public void DocumentFailedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.IdentityDocument;
        var failureReason = "OCR confidence too low";

        // Act
        var domainEvent = new DocumentFailedDomainEvent(aggregateId, version, providerId, documentType, failureReason);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.ProviderId.Should().Be(providerId);
        domainEvent.DocumentType.Should().Be(documentType);
        domainEvent.FailureReason.Should().Be(failureReason);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void DocumentRejectedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.ProofOfResidence;
        var rejectionReason = "Document expired";

        // Act
        var domainEvent = new DocumentRejectedDomainEvent(aggregateId, version, providerId, documentType, rejectionReason);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.ProviderId.Should().Be(providerId);
        domainEvent.DocumentType.Should().Be(documentType);
        domainEvent.RejectionReason.Should().Be(rejectionReason);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void DocumentUploadedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.CriminalRecord;
        var fileUrl = "https://storage.blob/documents/doc.pdf";

        // Act
        var domainEvent = new DocumentUploadedDomainEvent(aggregateId, version, providerId, documentType, fileUrl);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.ProviderId.Should().Be(providerId);
        domainEvent.DocumentType.Should().Be(documentType);
        domainEvent.FileUrl.Should().Be(fileUrl);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void DocumentVerifiedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 2;
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.IdentityDocument;
        var hasOcrData = true;

        // Act
        var domainEvent = new DocumentVerifiedDomainEvent(aggregateId, version, providerId, documentType, hasOcrData);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.ProviderId.Should().Be(providerId);
        domainEvent.DocumentType.Should().Be(documentType);
        domainEvent.HasOcrData.Should().BeTrue();
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void DocumentVerifiedDomainEvent_WithoutOcrData_ShouldInitializeCorrectly()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 2;
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.Other;

        // Act
        var domainEvent = new DocumentVerifiedDomainEvent(aggregateId, version, providerId, documentType, false);

        // Assert
        domainEvent.HasOcrData.Should().BeFalse();
        domainEvent.DocumentType.Should().Be(documentType);
    }
}
