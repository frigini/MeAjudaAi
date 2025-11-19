namespace MeAjudaAi.Shared.Contracts.Modules.Documents.DTOs;

/// <summary>
/// Contadores de documentos por status para um provider.
/// </summary>
public sealed record DocumentStatusCountDto
{
    /// <summary>
    /// Total de documentos.
    /// </summary>
    public required int Total { get; init; }

    /// <summary>
    /// Documentos com upload completo aguardando verificação.
    /// </summary>
    public required int Pending { get; init; }

    /// <summary>
    /// Documentos verificados e aprovados.
    /// </summary>
    public required int Verified { get; init; }

    /// <summary>
    /// Documentos rejeitados.
    /// </summary>
    public required int Rejected { get; init; }

    /// <summary>
    /// Documentos em processo de upload.
    /// </summary>
    public required int Uploading { get; init; }
}
