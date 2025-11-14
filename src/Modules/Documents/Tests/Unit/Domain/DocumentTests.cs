using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Events;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Domain;

public class DocumentTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateDocument()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.IdentityDocument;
        var fileName = "identity-card.pdf";
        var fileUrl = "https://storage.blob.core.windows.net/documents/identity-card.pdf";

        // Act
        var document = Document.Create(providerId, documentType, fileName, fileUrl);

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().NotBeNull();
        document.Id.Value.Should().NotBeEmpty();
        document.ProviderId.Should().Be(providerId);
        document.DocumentType.Should().Be(documentType);
        document.FileName.Should().Be(fileName);
        document.FileUrl.Should().Be(fileUrl);
        document.Status.Should().Be(EDocumentStatus.Uploaded);
        document.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.VerifiedAt.Should().BeNull();
        document.RejectionReason.Should().BeNull();
        document.OcrData.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseDocumentUploadedDomainEvent()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.ProofOfResidence;
        var fileName = "proof-residence.pdf";
        var fileUrl = "https://storage/proof-residence.pdf";

        // Act
        var document = Document.Create(providerId, documentType, fileName, fileUrl);

        // Assert
        document.DomainEvents.Should().HaveCount(1);
        var domainEvent = document.DomainEvents.First().Should().BeOfType<DocumentUploadedDomainEvent>().Subject;
        domainEvent.AggregateId.Should().Be(document.Id);
        domainEvent.ProviderId.Should().Be(providerId);
        domainEvent.DocumentType.Should().Be(documentType);
    }

    [Fact]
    public void MarkAsPendingVerification_ShouldUpdateStatus()
    {
        // Arrange
        var document = CreateTestDocument();

        // Act
        document.MarkAsPendingVerification();

        // Assert
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
    }

    [Fact]
    public void MarkAsPendingVerification_WhenNotUploaded_ShouldNotChangeStatus()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsVerified("{\"verified\": true}"); // Change to Verified status
        
        // Act
        document.MarkAsPendingVerification();
        
        // Assert
        document.Status.Should().Be(EDocumentStatus.Verified); // Should remain Verified
    }

    [Fact]
    public void MarkAsVerified_WithOcrData_ShouldUpdateStatusAndData()
    {
        // Arrange
        var document = CreateTestDocument();
        var ocrData = "{\"documentNumber\": \"123456789\"}";

        // Act
        document.MarkAsVerified(ocrData);

        // Assert
        document.Status.Should().Be(EDocumentStatus.Verified);
        document.VerifiedAt.Should().NotBeNull();
        document.VerifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.OcrData.Should().Be(ocrData);
        document.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void MarkAsVerified_ShouldRaiseDocumentVerifiedDomainEvent()
    {
        // Arrange
        var document = CreateTestDocument();
        document.ClearDomainEvents(); // Limpar evento de criação

        // Act
        document.MarkAsVerified("{\"verified\": true}");

        // Assert
        document.DomainEvents.Should().HaveCount(1);
        var domainEvent = document.DomainEvents.First().Should().BeOfType<DocumentVerifiedDomainEvent>().Subject;
        domainEvent.AggregateId.Should().Be(document.Id);
        domainEvent.ProviderId.Should().Be(document.ProviderId);
        domainEvent.DocumentType.Should().Be(document.DocumentType);
        domainEvent.HasOcrData.Should().BeTrue();
    }

    [Fact]
    public void MarkAsVerified_WhenAlreadyVerified_ShouldBeIdempotent()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsVerified("{\"first\": true}");
        document.ClearDomainEvents();
        
        // Act
        document.MarkAsVerified("{\"second\": true}");
        
        // Assert
        document.DomainEvents.Should().BeEmpty(); // No new events
        document.OcrData.Should().Be("{\"first\": true}"); // Data unchanged
    }

    [Fact]
    public void MarkAsRejected_WithReason_ShouldUpdateStatusAndReason()
    {
        // Arrange
        var document = CreateTestDocument();
        var rejectionReason = "Document is not legible";

        // Act
        document.MarkAsRejected(rejectionReason);

        // Assert
        document.Status.Should().Be(EDocumentStatus.Rejected);
        document.RejectionReason.Should().Be(rejectionReason);
        document.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsRejected_ShouldRaiseDocumentRejectedDomainEvent()
    {
        // Arrange
        var document = CreateTestDocument();
        document.ClearDomainEvents(); // Limpar evento de criação

        // Act
        document.MarkAsRejected("Invalid document");

        // Assert
        document.DomainEvents.Should().HaveCount(1);
        var domainEvent = document.DomainEvents.First().Should().BeOfType<DocumentRejectedDomainEvent>().Subject;
        domainEvent.AggregateId.Should().Be(document.Id);
        domainEvent.RejectionReason.Should().Be("Invalid document");
    }

    [Fact]
    public void MarkAsFailed_WithReason_ShouldUpdateStatusAndReason()
    {
        // Arrange
        var document = CreateTestDocument();
        var failureReason = "OCR service unavailable";

        // Act
        document.MarkAsFailed(failureReason);

        // Assert
        document.Status.Should().Be(EDocumentStatus.Failed);
        document.RejectionReason.Should().Be(failureReason);
    }

    [Theory]
    [InlineData(EDocumentType.IdentityDocument)]
    [InlineData(EDocumentType.ProofOfResidence)]
    [InlineData(EDocumentType.CriminalRecord)]
    [InlineData(EDocumentType.Other)]
    public void Create_WithDifferentDocumentTypes_ShouldCreateSuccessfully(EDocumentType documentType)
    {
        // Arrange & Act
        var document = Document.Create(
            Guid.NewGuid(),
            documentType,
            "test.pdf",
            "https://storage/test.pdf");

        // Assert
        document.DocumentType.Should().Be(documentType);
        document.Status.Should().Be(EDocumentStatus.Uploaded);
    }

    private static Document CreateTestDocument()
    {
        return Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test-document.pdf",
            "https://storage.blob.core.windows.net/documents/test-document.pdf");
    }
}
