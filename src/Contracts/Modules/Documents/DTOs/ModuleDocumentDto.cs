namespace MeAjudaAi.Contracts.Modules.Documents.DTOs;

/// <summary>
/// DTO de documento para comunicação entre módulos.
/// </summary>
/// <param name="Id">ID único do documento.</param>
/// <param name="ProviderId">ID do provider dono do documento.</param>
/// <param name="DocumentType">Tipo do documento (CPF, CNPJ, RG, CNH, etc).</param>
/// <param name="FileName">Nome do arquivo.</param>
/// <param name="FileUrl">URL do arquivo armazenado.</param>
/// <param name="Status">Status do documento (Uploaded, Pending, Verified, Rejected).</param>
/// <param name="UploadedAt">Data de upload.</param>
/// <param name="VerifiedAt">Data de verificação (se verificado).</param>
/// <param name="RejectionReason">Motivo da rejeição (se rejeitado).</param>
/// <param name="OcrData">Dados extraídos por OCR (se disponível).</param>
public sealed record ModuleDocumentDto(
    Guid Id,
    Guid ProviderId,
    string DocumentType,
    string FileName,
    string FileUrl,
    string Status,
    DateTime UploadedAt,
    DateTime? VerifiedAt = null,
    string? RejectionReason = null,
    string? OcrData = null);
