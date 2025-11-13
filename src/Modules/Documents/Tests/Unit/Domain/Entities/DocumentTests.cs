using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Events;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Domain.Entities;

public class DocumentTests
{
    private readonly Guid _providerId = Guid.NewGuid();
    private readonly string _fileUrl = "https://storage.example.com/documents/test.pdf";
    private readonly string _fileName = "test-document.pdf";

    [Fact]
    public void Constructor_ShouldCreateDocument_WithValidParameters()
    {
        // Act
        var document = new Document(
            _providerId,
            DocumentType.IdentityDocument,
            _fileUrl,
            _fileName);

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().NotBeEmpty();
        document.ProviderId.Should().Be(_providerId);
        document.DocumentType.Should().Be(DocumentType.IdentityDocument);
        document.FileUrl.Should().Be(_fileUrl);
        document.FileName.Should().Be(_fileName);
        document.Status.Should().Be(DocumentStatus.Uploaded);
        document.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.VerifiedAt.Should().BeNull();
        document.RejectionReason.Should().BeNull();
        document.OcrData.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldAddDocumentUploadedEvent()
    {
        // Act
        var document = new Document(
            _providerId,
            DocumentType.CriminalRecord,
            _fileUrl,
            _fileName);

        // Assert
        var domainEvents = document.GetDomainEvents();
        domainEvents.Should().HaveCount(1);
        domainEvents.First().Should().BeOfType<DocumentUploadedDomainEvent>();
        
        var uploadedEvent = (DocumentUploadedDomainEvent)domainEvents.First();
        uploadedEvent.DocumentId.Should().Be(document.Id);
        uploadedEvent.ProviderId.Should().Be(_providerId);
        uploadedEvent.DocumentType.Should().Be(DocumentType.CriminalRecord);
    }

    [Fact]
    public void MarkAsVerified_ShouldUpdateStatus_AndSetVerifiedAt()
    {
        // Arrange
        var document = new Document(_providerId, DocumentType.IdentityDocument, _fileUrl, _fileName);
        var ocrData = new { Name = "João Silva", DocumentNumber = "123456789" };

        // Act
        document.MarkAsVerified(ocrData);

        // Assert
        document.Status.Should().Be(DocumentStatus.Verified);
        document.VerifiedAt.Should().NotBeNull();
        document.VerifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.OcrData.Should().Be(ocrData);
        document.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void MarkAsVerified_ShouldAddDocumentVerifiedEvent()
    {
        // Arrange
        var document = new Document(_providerId, DocumentType.ProofOfResidence, _fileUrl, _fileName);
        document.ClearDomainEvents(); // Clear upload event
        var ocrData = new { Address = "Rua Teste, 123" };

        // Act
        document.MarkAsVerified(ocrData);

        // Assert
        var domainEvents = document.GetDomainEvents();
        domainEvents.Should().HaveCount(1);
        domainEvents.First().Should().BeOfType<DocumentVerifiedDomainEvent>();
        
        var verifiedEvent = (DocumentVerifiedDomainEvent)domainEvents.First();
        verifiedEvent.DocumentId.Should().Be(document.Id);
        verifiedEvent.ProviderId.Should().Be(_providerId);
        verifiedEvent.DocumentType.Should().Be(DocumentType.ProofOfResidence);
    }

    [Fact]
    public void MarkAsRejected_ShouldUpdateStatus_WithReason()
    {
        // Arrange
        var document = new Document(_providerId, DocumentType.CriminalRecord, _fileUrl, _fileName);
        var rejectionReason = "Document is expired";

        // Act
        document.MarkAsRejected(rejectionReason);

        // Assert
        document.Status.Should().Be(DocumentStatus.Rejected);
        document.RejectionReason.Should().Be(rejectionReason);
        document.VerifiedAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsRejected_ShouldAddDocumentRejectedEvent()
    {
        // Arrange
        var document = new Document(_providerId, DocumentType.IdentityDocument, _fileUrl, _fileName);
        document.ClearDomainEvents();
        var rejectionReason = "Invalid document format";

        // Act
        document.MarkAsRejected(rejectionReason);

        // Assert
        var domainEvents = document.GetDomainEvents();
        domainEvents.Should().HaveCount(1);
        domainEvents.First().Should().BeOfType<DocumentRejectedDomainEvent>();
        
        var rejectedEvent = (DocumentRejectedDomainEvent)domainEvents.First();
        rejectedEvent.DocumentId.Should().Be(document.Id);
        rejectedEvent.ProviderId.Should().Be(_providerId);
        rejectedEvent.Reason.Should().Be(rejectionReason);
    }

    [Fact]
    public void MarkAsPendingVerification_ShouldUpdateStatus()
    {
        // Arrange
        var document = new Document(_providerId, DocumentType.IdentityDocument, _fileUrl, _fileName);

        // Act
        document.MarkAsPendingVerification();

        // Assert
        document.Status.Should().Be(DocumentStatus.PendingVerification);
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatus_WithReason()
    {
        // Arrange
        var document = new Document(_providerId, DocumentType.IdentityDocument, _fileUrl, _fileName);
        var failureReason = "OCR service timeout";

        // Act
        document.MarkAsFailed(failureReason);

        // Assert
        document.Status.Should().Be(DocumentStatus.Failed);
        document.RejectionReason.Should().Be(failureReason);
    }

    [Theory]
    [InlineData(DocumentType.IdentityDocument)]
    [InlineData(DocumentType.ProofOfResidence)]
    [InlineData(DocumentType.CriminalRecord)]
    [InlineData(DocumentType.Other)]
    public void Constructor_ShouldAcceptAllDocumentTypes(DocumentType documentType)
    {
        // Act
        var document = new Document(_providerId, documentType, _fileUrl, _fileName);

        // Assert
        document.DocumentType.Should().Be(documentType);
        document.Status.Should().Be(DocumentStatus.Uploaded);
    }
}
