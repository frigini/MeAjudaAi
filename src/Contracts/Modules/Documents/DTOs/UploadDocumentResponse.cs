using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Contracts.Modules.Documents.DTOs;

/// <summary>
/// Response para geração de URL de upload de documento.
/// </summary>
/// <param name="DocumentId">ID do documento criado.</param>
/// <param name="UploadUrl">URL de upload com SAS token.</param>
/// <param name="BlobName">Nome do blob no Azure Storage.</param>
/// <param name="ExpiresAt">Data de expiração do SAS token.</param>
[ExcludeFromCodeCoverage]
public record UploadDocumentResponse(
    Guid DocumentId,
    string UploadUrl,
    string BlobName,
    DateTime ExpiresAt);
