using MeAjudaAi.Modules.Documents.API.Mappers;
using MeAjudaAi.Modules.Documents.Application.DTOs.Requests;
using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Layer", "API")]
public class RequestMapperExtensionsTests
{
    [Fact]
    public void ToCommand_UploadDocumentRequest_ShouldMapAllProperties()
    {
        // Arrange
        var request = new UploadDocumentRequest
        {
            ProviderId = Guid.NewGuid(),
            DocumentType = EDocumentType.IdentityDocument,
            FileName = "doc.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024
        };

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(request.ProviderId);
        command.DocumentType.Should().Be("IdentityDocument");
        command.FileName.Should().Be("doc.pdf");
        command.ContentType.Should().Be("application/pdf");
        command.FileSizeBytes.Should().Be(1024);
    }

    [Fact]
    public void ToCommand_UploadDocumentRequest_ProofOfResidence_ShouldMapEnumToString()
    {
        // Arrange
        var request = new UploadDocumentRequest
        {
            ProviderId = Guid.NewGuid(),
            DocumentType = EDocumentType.ProofOfResidence,
            FileName = "proof.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 2048
        };

        // Act
        var command = request.ToCommand();

        // Assert
        command.DocumentType.Should().Be("ProofOfResidence");
    }

    [Fact]
    public void ToApproveCommand_ShouldMapAllProperties()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var request = new VerifyDocumentRequest(IsVerified: true, VerificationNotes: "Documento válido");

        // Act
        var command = request.ToApproveCommand(documentId);

        // Assert
        command.Should().NotBeNull();
        command.DocumentId.Should().Be(documentId);
        command.VerificationNotes.Should().Be("Documento válido");
    }

    [Fact]
    public void ToApproveCommand_WithNullNotes_ShouldMapNull()
    {
        // Arrange
        var request = new VerifyDocumentRequest(IsVerified: true, VerificationNotes: null);

        // Act
        var command = request.ToApproveCommand(Guid.NewGuid());

        // Assert
        command.VerificationNotes.Should().BeNull();
    }

    [Fact]
    public void ToRejectCommand_ShouldMapAllProperties()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var request = new VerifyDocumentRequest(IsVerified: false, VerificationNotes: "Documento ilegível");

        // Act
        var command = request.ToRejectCommand(documentId);

        // Assert
        command.Should().NotBeNull();
        command.DocumentId.Should().Be(documentId);
        command.RejectionReason.Should().Be("Documento ilegível");
    }

    [Fact]
    public void ToRejectCommand_WithNullNotes_ShouldUseDefaultReason()
    {
        // Arrange
        var request = new VerifyDocumentRequest(IsVerified: false, VerificationNotes: null);

        // Act
        var command = request.ToRejectCommand(Guid.NewGuid());

        // Assert
        command.RejectionReason.Should().Be("Documento rejeitado durante verificação");
    }

    [Fact]
    public void ToRequestVerificationCommand_ShouldMapDocumentId()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        // Act
        var command = documentId.ToRequestVerificationCommand();

        // Assert
        command.Should().NotBeNull();
        command.DocumentId.Should().Be(documentId);
    }

    [Fact]
    public void ToQuery_GetDocumentStatus_ShouldMapDocumentId()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        // Act
        var query = documentId.ToQuery();

        // Assert
        query.Should().NotBeNull();
        query.DocumentId.Should().Be(documentId);
    }

    [Fact]
    public void ToDocumentsQuery_ShouldMapProviderId()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        // Act
        var query = providerId.ToDocumentsQuery();

        // Assert
        query.Should().NotBeNull();
        query.ProviderId.Should().Be(providerId);
    }
}
