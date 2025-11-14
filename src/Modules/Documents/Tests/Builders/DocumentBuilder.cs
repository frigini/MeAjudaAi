using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Tests.Builders;

/// <summary>
/// Builder pattern para criação de objetos Document em testes
/// </summary>
public class DocumentBuilder
{
    private Guid _providerId = Guid.NewGuid();
    private EDocumentType _documentType = EDocumentType.IdentityDocument;
    private string _fileName = "test-document.pdf";
    private string _fileUrl = "https://storage.blob.core.windows.net/documents/test-document.pdf";

    public DocumentBuilder WithProviderId(Guid providerId)
    {
        _providerId = providerId;
        return this;
    }

    public DocumentBuilder WithDocumentType(EDocumentType documentType)
    {
        _documentType = documentType;
        return this;
    }

    public DocumentBuilder WithFileName(string fileName)
    {
        _fileName = fileName;
        return this;
    }

    public DocumentBuilder WithFileUrl(string fileUrl)
    {
        _fileUrl = fileUrl;
        return this;
    }

    public DocumentBuilder AsIdentityDocument()
    {
        _documentType = EDocumentType.IdentityDocument;
        _fileName = "identity-card.pdf";
        return this;
    }

    public DocumentBuilder AsProofOfResidence()
    {
        _documentType = EDocumentType.ProofOfResidence;
        _fileName = "proof-residence.pdf";
        return this;
    }

    public DocumentBuilder AsCriminalRecord()
    {
        _documentType = EDocumentType.CriminalRecord;
        _fileName = "criminal-record.pdf";
        return this;
    }

    public Document Build()
    {
        return Document.Create(_providerId, _documentType, _fileName, _fileUrl);
    }

    public static DocumentBuilder ADocument() => new();
}
