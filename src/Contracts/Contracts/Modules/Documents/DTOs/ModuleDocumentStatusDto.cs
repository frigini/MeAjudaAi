namespace MeAjudaAi.Contracts.Modules.Documents.DTOs;

/// <summary>
/// DTO simplificado com apenas status do documento.
/// </summary>
/// <param name="DocumentId">ID do documento.</param>
/// <param name="Status">Status atual do documento. Valid values: "PendingVerification", "Verified", "Rejected"</param>
/// <param name="UpdatedAt">Data da última atualização.</param>
public sealed record ModuleDocumentStatusDto(
    Guid DocumentId,
    string Status,
    DateTime UpdatedAt);
