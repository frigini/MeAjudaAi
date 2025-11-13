using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Application.DTOs;

public record DocumentDto(
    Guid Id,
    Guid ProviderId,
    DocumentType DocumentType,
    string FileName,
    string FileUrl,
    DocumentStatus Status,
    DateTime UploadedAt,
    DateTime? VerifiedAt,
    string? RejectionReason,
    string? OcrData);
