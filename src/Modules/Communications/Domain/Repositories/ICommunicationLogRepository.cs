using MeAjudaAi.Modules.Communications.Domain.Entities;

namespace MeAjudaAi.Modules.Communications.Domain.Repositories;

/// <summary>
/// Repositório de logs de comunicação.
/// </summary>
public interface ICommunicationLogRepository
{
    /// <summary>
    /// Adiciona um novo log.
    /// </summary>
    Task AddAsync(CommunicationLog log, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se já existe um log com o CorrelationId especificado.
    /// Usado para garantir idempotência.
    /// </summary>
    Task<bool> ExistsByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna o histórico de logs de um determinado destinatário.
    /// </summary>
    Task<IReadOnlyList<CommunicationLog>> GetByRecipientAsync(
        string recipient,
        int maxResults = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca logs de comunicação de forma paginada.
    /// </summary>
    Task<(IReadOnlyList<CommunicationLog> Items, int TotalCount)> SearchAsync(
        string? correlationId = null,
        string? channel = null,
        string? recipient = null,
        bool? isSuccess = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste as alterações no banco de dados.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
