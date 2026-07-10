using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Documents;

[ExcludeFromCodeCoverage]
public class DocumentBuilder : BaseBuilder<Document>
{
    private Guid? _providerId;
    private EDocumentType? _documentType;
    private string? _fileName;
    private string? _fileUrl;

    public DocumentBuilder()
    {
        Faker = new Faker<Document>()
            .CustomInstantiator(f =>
            {
                return Document.Create(
                    _providerId ?? f.Random.Guid(),
                    _documentType ?? f.PickRandom(
                        EDocumentType.IdentityDocument,
                        EDocumentType.ProofOfResidence,
                        EDocumentType.CriminalRecord,
                        EDocumentType.Other),
                    _fileName ?? f.System.FileName(),
                    _fileUrl ?? $"blobs/{f.Random.Guid():N}.pdf"
                );
            });
    }

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
        return this;
    }

    public DocumentBuilder AsProofOfResidence()
    {
        _documentType = EDocumentType.ProofOfResidence;
        return this;
    }

    public DocumentBuilder AsCriminalRecord()
    {
        _documentType = EDocumentType.CriminalRecord;
        return this;
    }
}
