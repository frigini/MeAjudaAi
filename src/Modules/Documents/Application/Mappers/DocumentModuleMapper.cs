using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;

namespace MeAjudaAi.Modules.Documents.Application.Mappers;

[ExcludeFromCodeCoverage]
public static class DocumentModuleMapper
{
    public static ModuleDocumentDto ToModuleDto(this DocumentDto dto) => new(
        dto.Id,
        dto.ProviderId,
        dto.DocumentType.ToString(),
        dto.FileName,
        dto.FileUrl,
        dto.Status.ToString(),
        dto.UploadedAt,
        dto.VerifiedAt,
        dto.RejectionReason,
        dto.OcrData);
}
