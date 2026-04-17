using MeAjudaAi.Modules.Providers.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para documento.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DocumentDto(
    string Number,
    EDocumentType DocumentType,
    string? FileName,
    string? FileUrl,
    bool IsPrimary
);
