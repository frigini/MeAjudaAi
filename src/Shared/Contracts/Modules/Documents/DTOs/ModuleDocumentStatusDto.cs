namespace MeAjudaAi.Shared.Contracts.Modules.Documents.DTOs;

/// <summary>
/// DTO simplificado com apenas status do documento.
/// </summary>
public sealed record ModuleDocumentStatusDto
{
    /// <summary>
    /// ID do documento.
    /// </summary>
    public required Guid DocumentId { get; init; }

    /// <summary>
    /// Status atual do documento.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Data da última atualização.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }
}
