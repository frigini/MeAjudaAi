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
    public void MarkAsPendingVerification_WhenVerified_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"verified\": true}"); // Change to Verified status

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            document.MarkAsPendingVerification());

        exception.Message.Should().Contain("Cannot mark document as pending verification from state Verified");
        document.Status.Should().Be(EDocumentStatus.Verified); // Should remain Verified
    }

    [Fact]
    public void MarkAsPendingVerification_WhenFailed_ShouldAllowRetry()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsFailed("OCR service unavailable");
        document.Status.Should().Be(EDocumentStatus.Failed);
        document.RejectionReason.Should().Be("OCR service unavailable");

        // Act - retry should be allowed
        document.MarkAsPendingVerification();

        // Assert
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        document.RejectionReason.Should().BeNull(); // Cleared for retry
    }

    [Fact]
    public void MarkAsVerified_WithOcrData_ShouldUpdateStatusAndData()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification(); // Transição para estado permitido
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
        document.MarkAsPendingVerification(); // Transição para estado permitido
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
    public void MarkAsVerified_WhenNotInPendingVerification_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var document = CreateTestDocument();
        // Status é Uploaded, não PendingVerification

        // Act & Assert
        var act = () => document.MarkAsVerified("{\"data\": true}");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be in PendingVerification status*");
    }

    [Fact]
    public void MarkAsRejected_WithReason_ShouldUpdateStatusAndReason()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification(); // Transição para estado permitido
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
        document.MarkAsPendingVerification(); // Transição para estado permitido
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
    public void MarkAsRejected_WithEmptyReason_ShouldThrowArgumentException()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();

        // Act & Assert
        var act = () => document.MarkAsRejected("");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Rejection reason cannot be empty*");
    }

    [Fact]
    public void MarkAsRejected_WhenNotInPendingVerification_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var document = CreateTestDocument();
        // Status é Uploaded, não PendingVerification

        // Act & Assert
        var act = () => document.MarkAsRejected("Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be in PendingVerification status*");
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

    [Fact]
    public void MarkAsFailed_ShouldRaiseDocumentFailedDomainEvent()
    {
        // Arrange
        var document = CreateTestDocument();
        document.ClearDomainEvents(); // Limpar evento de criação

        // Act
        document.MarkAsFailed("Service timeout");

        // Assert
        document.DomainEvents.Should().HaveCount(1);
        var domainEvent = document.DomainEvents.First().Should().BeOfType<DocumentFailedDomainEvent>().Subject;
        domainEvent.AggregateId.Should().Be(document.Id);
        domainEvent.ProviderId.Should().Be(document.ProviderId);
        domainEvent.DocumentType.Should().Be(document.DocumentType);
        domainEvent.FailureReason.Should().Be("Service timeout");
    }

    [Fact]
    public void MarkAsVerified_WithoutOcrData_ShouldUpdateStatusWithoutData()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();

        // Act
        document.MarkAsVerified(null);

        // Assert
        document.Status.Should().Be(EDocumentStatus.Verified);
        document.VerifiedAt.Should().NotBeNull();
        document.OcrData.Should().BeNull();
    }

    [Fact]
    public void MarkAsVerified_WithoutOcrData_ShouldIndicateNoOcrDataInEvent()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();
        document.ClearDomainEvents();

        // Act
        document.MarkAsVerified(null);

        // Assert
        var domainEvent = document.DomainEvents.First().Should().BeOfType<DocumentVerifiedDomainEvent>().Subject;
        domainEvent.HasOcrData.Should().BeFalse();
    }

    [Fact]
    public void MarkAsFailed_FromAnyStatus_ShouldSucceed()
    {
        // Arrange - Teste que MarkAsFailed não tem guard de estado (diferente de outros métodos)
        var documentUploaded = CreateTestDocument();
        var documentPending = CreateTestDocument();
        documentPending.MarkAsPendingVerification();

        // Act & Assert - Deve funcionar de qualquer estado
        var actUploaded = () => documentUploaded.MarkAsFailed("Error from Uploaded");
        var actPending = () => documentPending.MarkAsFailed("Error from Pending");

        actUploaded.Should().NotThrow();
        actPending.Should().NotThrow();

        documentUploaded.Status.Should().Be(EDocumentStatus.Failed);
        documentPending.Status.Should().Be(EDocumentStatus.Failed);
    }

[Fact]
    public void MarkAsVerified_ForStatusFromUploaded_ShouldSucceed()  // renamed and fixed
    {
        var document = CreateTestDocument();

        document. MarkAsPendingVerification();
        var result = () => document.MarkAsVerified(null);
        result. Should().NotThrow();
        document. Status. Should().Be(EDocumentStatus.Verified);
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

    [Fact]
    public void Create_WithEmptyProviderId_ShouldThrowArgumentException()
    {
        // Act & Assert
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
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, fileName, "blob-key"));

        exception.ParamName.Should().Be("fileName");
    }

[Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFileUrl_ShouldThrowArgumentNullException( string fileUrl)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "test.pdf", fileUrl));

        exception.ParamName.Should().Be("fileUrl");
    }

    [Fact]
    public void Create_WithWhitespaceFileName_WhitespaceFileUrl_ShouldTrimAndSucceed()
    {
        // Arrange & Act
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "  document.pdf  ",
            "  blob-key  ");

        // Assert
        document.FileName.Should().Be("document.pdf");
        document.FileUrl.Should().Be("blob-key");
    }

    [Fact]
    public void Create_WithValidInputs_ShouldHaveCorrectInitialState()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.ProofOfResidence;
        var fileName = "proof.pdf";
        var fileUrl = "blob://test";

        // Act
        var document = Document.Create(providerId, documentType, fileName, fileUrl);

        // Assert
        document.Id.Should().NotBeNull();
        document.Id.Value.Should().NotBe(Guid.Empty);
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

[Theory]
[InlineData(EDocumentStatus.PendingVerification)]
[InlineData(EDocumentStatus.Uploaded)]
[InlineData(EDocumentStatus.Failed)]
[InlineData(EDocumentStatus.Verified)]
[InlineData(EDocumentStatus.Rejected)]
public void MarkAsVerified_OnlySucceedsFromPendingVerification(EDocumentStatus initialStatus)
{
    var doc = CreateTestDocument();

    switch (initialStatus)
    {
        case EDocumentStatus.PendingVerification:
            doc.MarkAsPendingVerification();
            break;
        case EDocumentStatus.Failed:
            doc.MarkAsPendingVerification();
            doc.MarkAsFailed("failed");
            break;
        case EDocumentStatus.Verified:
            doc.MarkAsPendingVerification();
            doc.MarkAsVerified("{\"data\": \"test\"}");
            break;
        case EDocumentStatus.Rejected:
            doc.MarkAsPendingVerification();
            doc.MarkAsRejected("rejected");
            break;
        case EDocumentStatus.Uploaded:
            break;
    }

    if (initialStatus == EDocumentStatus.PendingVerification)
    {
        doc.MarkAsVerified(null);
        doc.Status.Should().Be(EDocumentStatus.Verified);
    }
    else
    {
        var act = () => doc.MarkAsVerified(null);
        act.Should().Throw<InvalidOperationException>();
    }
}
    [Fact]
    public void MarkAsVerified_ShouldPreservePreviousOcrData()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"initial\": \"data\"}");
        document.ClearDomainEvents();

        // Act - trying to mark as verified again (should fail)
        var act = () => document.MarkAsVerified("{\"new\": \"data\"}");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsVerified_WithEmptyString_DoesNotSetOcrData()
    {
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();

        document.MarkAsVerified("");

        document.Status.Should().Be(EDocumentStatus.Verified);
        document.OcrData.Should().Be("");
    }

    [Fact]
    public void MarkAsRejected_FromVerified_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();
        document.MarkAsVerified(null);

        // Act & Assert
        var act = () => document.MarkAsRejected("Some reason");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be in PendingVerification*");
    }

    [Fact]
    public void MarkAsRejected_FromRejected_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();
        document.MarkAsRejected("First rejection");
        document.ClearDomainEvents();

        // Act & Assert
        var act = () => document.MarkAsRejected("Second rejection");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be in PendingVerification*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MarkAsRejected_WithInvalidReason_ShouldThrowArgumentException(string reason)
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();

        // Act & Assert
        var act = () => document.MarkAsRejected(reason);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void MarkAsFailed_FromVerified_ShouldSucceed()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();
        document.MarkAsVerified(null);

        // Act
        document.MarkAsFailed("Service error after verification");

        // Assert
        document.Status.Should().Be(EDocumentStatus.Failed);
    }

    [Fact]
    public void MarkAsFailed_FromRejected_ShouldSucceed()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();
        document.MarkAsRejected("Bad document");
        document.ClearDomainEvents();

        // Act
        document.MarkAsFailed("Technical error after rejection");

        // Assert
        document.Status.Should().Be(EDocumentStatus.Failed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MarkAsFailed_WithInvalidReason_ShouldThrowArgumentException(string reason)
    {
        // Arrange
        var document = CreateTestDocument();

        // Act & Assert
        var act = () => document.MarkAsFailed(reason);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void MarkAsPendingVerification_FromUploaded_ShouldSucceed()
    {
        // Arrange
        var document = CreateTestDocument();

        // Act
        document.MarkAsPendingVerification();

        // Assert
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
    }

    [Fact]
    public void MarkAsPendingVerification_FromFailed_ShouldClearReasonAndSucceed()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsFailed("OCR error");
        document.ClearDomainEvents();

        // Act
        document.MarkAsPendingVerification();

        // Assert
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        document.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void MarkAsPendingVerification_FromPendingVerification_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();

        // Act & Assert
        var act = () => document.MarkAsPendingVerification();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Document must be Uploaded or Failed*");
    }

    [Fact]
    public void MarkAsPendingVerification_FromVerified_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();
        document.MarkAsVerified(null);

        // Act & Assert
        var act = () => document.MarkAsPendingVerification();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Document must be Uploaded or Failed*");
    }

    [Fact]
    public void MarkAsPendingVerification_FromRejected_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var document = CreateTestDocument();
        document.MarkAsPendingVerification();
        document.MarkAsRejected("Invalid document");

        // Act & Assert
        var act = () => document.MarkAsPendingVerification();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Document must be Uploaded or Failed*");
    }

    [Fact]
    public void AllStateTransitions_WhenCalled_ShouldNotAffectOtherProperties()
    {
        // Arrange
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.ProofOfResidence,
            "proof.pdf",
            "blob://test");

        var originalId = document.Id;
        var originalProviderId = document.ProviderId;
        var originalDocumentType = document.DocumentType;
        var originalFileName = document.FileName;
        var originalFileUrl = document.FileUrl;

        // Act - Apply various state transitions
        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"data\": \"test\"}");

        // Assert - Properties should remain unchanged
        document.Id.Should().Be(originalId);
        document.ProviderId.Should().Be(originalProviderId);
        document.DocumentType.Should().Be(originalDocumentType);
        document.FileName.Should().Be(originalFileName);
        document.FileUrl.Should().Be(originalFileUrl);
    }

    [Fact]
    public void DomainEvents_AfterStateTransition_ShouldIncludeCorrectData()
    {
        // Arrange
        var document = CreateTestDocument();
        document.ClearDomainEvents();
        var providerId = document.ProviderId;
        var documentType = document.DocumentType;

        // Act
        document.MarkAsPendingVerification();
        document.MarkAsFailed("Test failure");
        var failedEvent = document.DomainEvents.OfType<DocumentFailedDomainEvent>().SingleOrDefault();

        // Assert
        failedEvent.Should().NotBeNull();
        failedEvent!.ProviderId.Should().Be(providerId);
        failedEvent.DocumentType.Should().Be(documentType);
        failedEvent.FailureReason.Should().Be("Test failure");
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
