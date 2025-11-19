namespace MeAjudaAi.Modules.Documents.Application.DTOs;

public record UploadDocumentResponse(
    Guid DocumentId,
    string UploadUrl,
    string BlobName,
    DateTime ExpiresAt);
