namespace MeAjudaAi.Shared.Contracts.Modules.Documents.DTOs;

/// <summary>
/// DTO de documento para comunicação entre módulos.
/// </summary>
public sealed record ModuleDocumentDto
{
    /// <summary>
    /// ID único do documento.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// ID do provider dono do documento.
    /// </summary>
    public required Guid ProviderId { get; init; }

    /// <summary>
    /// Tipo do documento (CPF, CNPJ, RG, CNH, etc).
    /// </summary>
    public required string DocumentType { get; init; }

    /// <summary>
    /// Nome do arquivo.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// URL do arquivo armazenado.
    /// </summary>
    public required string FileUrl { get; init; }

    /// <summary>
    /// Status do documento (Uploaded, Pending, Verified, Rejected).
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Data de upload.
    /// </summary>
    public required DateTime UploadedAt { get; init; }

    /// <summary>
    /// Data de verificação (se verificado).
    /// </summary>
    public DateTime? VerifiedAt { get; init; }

    /// <summary>
    /// Motivo da rejeição (se rejeitado).
    /// </summary>
    public string? RejectionReason { get; init; }

    /// <summary>
    /// Dados extraídos por OCR (se disponível).
    /// </summary>
    public string? OcrData { get; init; }
}
