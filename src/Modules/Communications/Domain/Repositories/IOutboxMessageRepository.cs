using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;

namespace MeAjudaAi.Modules.Communications.Domain.Repositories;

/// <summary>
/// Repositório de mensagens do Outbox.
/// </summary>
public interface IOutboxMessageRepository
{
    /// <summary>
    /// Adiciona uma nova mensagem ao Outbox.
    /// </summary>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna as próximas mensagens prontas para processamento,
    /// ordenadas por prioridade e data de criação.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna uma mensagem pelo ID.
    /// </summary>
    Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta mensagens com um determinado status.
    /// </summary>
    Task<int> CountByStatusAsync(EOutboxMessageStatus status, CancellationToken cancellationToken = default);
}
