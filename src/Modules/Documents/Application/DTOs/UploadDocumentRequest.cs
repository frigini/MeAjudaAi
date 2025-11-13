using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Application.DTOs;

public record UploadDocumentRequest(
    Guid ProviderId,
    DocumentType DocumentType,
    string FileName,
    string ContentType,
    long FileSizeBytes);
