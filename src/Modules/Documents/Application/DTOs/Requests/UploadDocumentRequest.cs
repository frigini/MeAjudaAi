using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Documents.Application.DTOs.Requests;

/// <summary>
/// Request para geração de URL de upload de documento.
/// </summary>
public record UploadDocumentRequest
{
    /// <summary>
    /// ID do prestador que está enviando o documento.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Tipo do documento (IdentityDocument, ProofOfResidence, CriminalRecord, Other).
    /// </summary>
    public EDocumentType DocumentType { get; init; }

    /// <summary>
    /// Nome do arquivo.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Tipo de conteúdo (MIME type).
    /// </summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>
    /// Tamanho do arquivo em bytes.
    /// </summary>
    public long FileSizeBytes { get; init; }
}
