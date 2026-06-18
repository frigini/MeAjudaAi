using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Contracts.Modules.Documents.DTOs;

/// <summary>
/// Request para geração de URL de upload de documento.
/// </summary>
/// <param name="ProviderId">ID do prestador que está enviando o documento.</param>
/// <param name="DocumentType">Tipo do documento (IdentityDocument, ProofOfResidence, CriminalRecord, Other).</param>
/// <param name="FileName">Nome do arquivo.</param>
/// <param name="ContentType">Tipo de conteúdo (MIME type).</param>
/// <param name="FileSizeBytes">Tamanho do arquivo em bytes.</param>
[ExcludeFromCodeCoverage]
public record UploadDocumentRequest(
    Guid ProviderId,
    string DocumentType,
    string FileName,
    string ContentType,
    long FileSizeBytes);
