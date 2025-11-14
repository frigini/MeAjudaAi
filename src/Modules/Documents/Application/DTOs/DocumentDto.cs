using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Application.DTOs;

public record DocumentDto(
    Guid Id,
    Guid ProviderId,
    EDocumentType DocumentType,
    string FileName,
    string FileUrl,
    EDocumentStatus Status,
    DateTime UploadedAt,
    DateTime? VerifiedAt,
    string? RejectionReason,
    string? OcrData);
