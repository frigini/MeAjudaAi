using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Events;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Domain;

[Trait("Category", "Unit")]
public class DocumentTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateDocument()
    {
        // Preparação
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.IdentityDocument;
        var fileName = "identity-card.pdf";
        var fileUrl = "https://storage.blob.core.windows.net/documents/identity-card.pdf";

        // Ação
        var document = Document.Create(providerId, documentType, fileName, fileUrl);

        // Verificação
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
        // Preparação
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.ProofOfResidence;
        var fileName = "proof-residence.pdf";
        var fileUrl = "https://storage/proof-residence.pdf";

        // Ação
        var document = Document.Create(providerId, documentType, fileName, fileUrl);

        // Verificação
        document.DomainEvents.Should().HaveCount(1);
        var domainEvent = document.DomainEvents.First().Should().BeOfType<DocumentUploadedDomainEvent>().Subject;
        domainEvent.AggregateId.Should().Be(document.Id);
        domainEvent.ProviderId.Should().Be(providerId);
        domainEvent.DocumentType.Should().Be(documentType);
    }

    [Fact]
    public void MarkAsPendingVerification_ShouldUpdateStatus()
    {
        // Preparação
        var document = CreateTestDocument();

        // Ação
        document.MarkAsPendingVerification();

        // Verificação
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
    }

    [Fact]
    public void MarkAsPendingVerification_WhenVerified_ShouldThrowInvalidOperationException()
    {
        // Preparação
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"verified\": true}"); // Change to Verified status

        // Ação & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            document.MarkAsPendingVerification());

        exception.Message.Should().Contain("Cannot mark document as pending verification from state Verified");
        document.Status.Should().Be(EDocumentStatus.Verified); // Should remain Verified
    }

    [Fact]
    public void MarkAsPendingVerification_WhenFailed_ShouldAllowRetry()
    {
        // Preparação
        var document = CreateTestDocument();
        document.MarkAsFailed("OCR service unavailable");
        document.Status.Should().Be(EDocumentStatus.Failed);
        document.RejectionReason.Should().Be("OCR service unavailable");

        // Ação - retry should be allowed
        document.MarkAsPendingVerification();

        // Verificação
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        document.RejectionReason.Should().BeNull(); // Cleared for retry
    }

    [Fact]
    public void MarkAsVerified_WithOcrData_ShouldUpdateStatusAndData()
    {
        // Preparação
        var document = CreateTestDocument();
        document.MarkAsPendingVerification(); // Transição para estado permitido
        var ocrData = "{\"documentNumber\": \"123456789\"}";

        // Ação
        document.MarkAsVerified(ocrData);

        // Verificação
        document.Status.Should().Be(EDocumentStatus.Verified);
        document.VerifiedAt.Should().NotBeNull();
        document.VerifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.OcrData.Should().Be(ocrData);
        document.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void MarkAsVerified_ShouldRaiseDocumentVerifiedDomainEvent()
    {
        // Preparação
        var document = CreateTestDocument();
        document.MarkAsPendingVerification(); // Transição para estado permitido
        document.ClearDomainEvents(); // Limpar evento de criação

        // Ação
        document.MarkAsVerified("{\"verified\": true}");

        // Verificação
        document.DomainEvents.Should().HaveCount(1);
        var domainEvent = document.DomainEvents.First().Should().BeOfType<DocumentVerifiedDomainEvent>().Subject;
        domainEvent.AggregateId.Should().Be(document.Id);
        domainEvent.ProviderId.Should().Be(document.ProviderId);
        domainEvent.DocumentType.Should().Be(document.DocumentType);
        domainEvent.HasOcrData.Should().BeTrue();
    }

    [Fact]
    public void MarkAsVerified_WhenNotInPendingVerification_ShouldThrowInvalidOperationException()
    {
        // Preparação
        var document = CreateTestDocument();
        // Status é Uploaded, não PendingVerification

        // Ação & Assert
        var act = () => document.MarkAsVerified("{\"data\": true}");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be in PendingVerification status*");
    }

    [Fact]
    public void MarkAsRejected_WithReason_ShouldUpdateStatusAndReason()
    {
        // Preparação
        var document = CreateTestDocument();
        document.MarkAsPendingVerification(); // Transição para estado permitido
        var rejectionReason = "Document is not legible";

        // Ação
        document.MarkAsRejected(rejectionReason);

        // Verificação
        document.Status.Should().Be(EDocumentStatus.Rejected);
        document.RejectionReason.Should().Be(rejectionReason);
        document.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsRejected_ShouldRaiseDocumentRejectedDomainEvent()
    {
        // Preparação
        var document = CreateTestDocument();
        document.MarkAsPendingVerification(); // Transição para estado permitido
        document.ClearDomainEvents(); // Limpar evento de criação

        // Ação
        document.MarkAsRejected("Invalid document");

        // Verificação
        document.DomainEvents.Should().HaveCount(1);
        var domainEvent = document.DomainEvents.First().Should().BeOfType<DocumentRejectedDomainEvent>().Subject;
        domainEvent.AggregateId.Should().Be(document.Id);
        domainEvent.RejectionReason.Should().Be("Invalid document");
    }

    [Fact]
    public void MarkAsRejected_WithEmptyReason_ShouldThrowArgumentException()
    {
        // Preparação
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();

        // Ação & Assert
        var act = () => document.MarkAsRejected("");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Rejection reason cannot be empty*");
    }

    [Fact]
    public void MarkAsRejected_WhenNotInPendingVerification_ShouldThrowInvalidOperationException()
    {
        // Preparação
        var document = CreateTestDocument();
        // Status é Uploaded, não PendingVerification

        // Ação & Assert
        var act = () => document.MarkAsRejected("Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be in PendingVerification status*");
    }

    [Fact]
    public void MarkAsFailed_WithReason_ShouldUpdateStatusAndReason()
    {
        // Preparação
        var document = CreateTestDocument();
        var failureReason = "OCR service unavailable";

        // Ação
        document.MarkAsFailed(failureReason);

        // Verificação
        document.Status.Should().Be(EDocumentStatus.Failed);
        document.RejectionReason.Should().Be(failureReason);
    }

    [Fact]
    public void MarkAsFailed_WithEmptyReason_ShouldThrowArgumentException()
    {
        // Preparação
        var document = CreateTestDocument();

        // Ação & Assert
        var act = () => document.MarkAsFailed("");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Failure reason cannot be empty*");
    }

    [Theory]
    [InlineData(EDocumentType.IdentityDocument)]
    [InlineData(EDocumentType.ProofOfResidence)]
    [InlineData(EDocumentType.CriminalRecord)]
    [InlineData(EDocumentType.Other)]
    public void Create_WithDifferentDocumentTypes_ShouldCreateSuccessfully(EDocumentType documentType)
    {
        // Preparação & Act
        var document = Document.Create(
            Guid.NewGuid(),
            documentType,
            "test.pdf",
            "https://storage/test.pdf");

        // Verificação
        document.DocumentType.Should().Be(documentType);
        document.Status.Should().Be(EDocumentStatus.Uploaded);
    }

    [Fact]
    public void Create_WithEmptyProviderId_ShouldThrowArgumentException()
    {
        // Ação & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Document.Create(Guid.Empty, EDocumentType.IdentityDocument, "test.pdf", "blob-key"));

        exception.ParamName.Should().Be("providerId");
        exception.Message.Should().Contain("Provider ID cannot be empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFileName_ShouldThrowArgumentNullException(string fileName)
    {
        // Ação & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, fileName, "blob-key"));

        exception.ParamName.Should().Be("fileName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFileUrl_ShouldThrowArgumentNullException(string fileUrl)
    {
        // Ação & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "test.pdf", fileUrl));

        exception.ParamName.Should().Be("fileUrl");
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
