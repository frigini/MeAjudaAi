using MeAjudaAi.Modules.Documents.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Documents.Application.DTOs;

[ExcludeFromCodeCoverage]

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
