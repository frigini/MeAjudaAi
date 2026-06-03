namespace MeAjudaAi.Contracts.Modules.Documents.DTOs;

/// <summary>
/// Contadores de documentos por status para um provider.
/// </summary>
/// <param name="Total">Total de documentos.</param>
/// <param name="Pending">Documentos com upload completo aguardando verificação.</param>
/// <param name="Verified">Documentos verificados e aprovados.</param>
/// <param name="Rejected">Documentos rejeitados.</param>
/// <param name="Uploading">Documentos em processo de upload.</param>
public sealed record DocumentStatusCountDto(
    int Total,
    int Pending,
    int Verified,
    int Rejected,
    int Uploading);
