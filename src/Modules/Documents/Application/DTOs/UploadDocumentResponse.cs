using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Modules.Documents.Application.DTOs;

[ExcludeFromCodeCoverage]

public record UploadDocumentResponse(
    Guid DocumentId,
    string UploadUrl,
    string BlobName,
    DateTime ExpiresAt);
