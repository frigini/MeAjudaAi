using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para documento.
/// </summary>
public sealed record DocumentDto(
    string Number,
    EDocumentType DocumentType,
    string? FileName,
    string? FileUrl,
    bool IsPrimary
);
