using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Tests.Builders;

/// <summary>
/// Test builder for Document aggregate following the Builder pattern
/// </summary>
public class DocumentBuilder
{
    private Guid _providerId = Guid.NewGuid();
    private DocumentType _documentType = DocumentType.IdentityDocument;
    private string _fileUrl = "https://storage.example.com/test.pdf";
    private string _fileName = "test-document.pdf";
    private DocumentStatus? _status;
    private DateTime? _verifiedAt;
    private string? _rejectionReason;
    private object? _ocrData;

    public DocumentBuilder WithProviderId(Guid providerId)
    {
        _providerId = providerId;
        return this;
    }

    public DocumentBuilder WithDocumentType(DocumentType documentType)
    {
        _documentType = documentType;
        return this;
    }

    public DocumentBuilder WithFileUrl(string fileUrl)
    {
        _fileUrl = fileUrl;
        return this;
    }

    public DocumentBuilder WithFileName(string fileName)
    {
        _fileName = fileName;
        return this;
    }

    public DocumentBuilder WithVerifiedStatus(object? ocrData = null)
    {
        _status = DocumentStatus.Verified;
        _verifiedAt = DateTime.UtcNow;
        _ocrData = ocrData ?? new { Verified = true };
        return this;
    }

    public DocumentBuilder WithRejectedStatus(string reason)
    {
        _status = DocumentStatus.Rejected;
        _rejectionReason = reason;
        return this;
    }

    public DocumentBuilder WithPendingVerificationStatus()
    {
        _status = DocumentStatus.PendingVerification;
        return this;
    }

    public DocumentBuilder WithFailedStatus(string reason)
    {
        _status = DocumentStatus.Failed;
        _rejectionReason = reason;
        return this;
    }

    public Document Build()
    {
        var document = new Document(_providerId, _documentType, _fileUrl, _fileName);

        if (_status.HasValue)
        {
            switch (_status.Value)
            {
                case DocumentStatus.Verified:
                    document.MarkAsVerified(_ocrData);
                    break;
                case DocumentStatus.Rejected:
                    document.MarkAsRejected(_rejectionReason!);
                    break;
                case DocumentStatus.PendingVerification:
                    document.MarkAsPendingVerification();
                    break;
                case DocumentStatus.Failed:
                    document.MarkAsFailed(_rejectionReason!);
                    break;
            }
        }

        return document;
    }

    /// <summary>
    /// Creates a document builder with default identity document settings
    /// </summary>
    public static DocumentBuilder IdentityDocument()
    {
        return new DocumentBuilder()
            .WithDocumentType(DocumentType.IdentityDocument)
            .WithFileName("identity-document.pdf");
    }

    /// <summary>
    /// Creates a document builder with default proof of residence settings
    /// </summary>
    public static DocumentBuilder ProofOfResidence()
    {
        return new DocumentBuilder()
            .WithDocumentType(DocumentType.ProofOfResidence)
            .WithFileName("proof-of-residence.pdf");
    }

    /// <summary>
    /// Creates a document builder with default criminal record settings
    /// </summary>
    public static DocumentBuilder CriminalRecord()
    {
        return new DocumentBuilder()
            .WithDocumentType(DocumentType.CriminalRecord)
            .WithFileName("criminal-record.pdf");
    }
}
