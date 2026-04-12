using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Contracts.Shared;
using OutboxMessage = MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage;

namespace MeAjudaAi.Modules.Communications.Domain.Repositories;

/// <summary>
/// Repositório para gestão de mensagens Outbox de comunicação.
/// Herda da interface genérica para garantir consistência no processamento.
/// </summary>
public interface IOutboxMessageRepository : IOutboxRepository<OutboxMessage>
{
    /// <summary>
    /// Retorna o total de mensagens por status (usado para monitoramento/health checks).
    /// </summary>
    Task<int> CountByStatusAsync(EOutboxMessageStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpa mensagens muito antigas já enviadas para economizar espaço em disco.
    /// </summary>
    Task<int> CleanupOldMessagesAsync(DateTime threshold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reseta mensagens travadas no status 'Processing' por muito tempo.
    /// </summary>
    Task<int> ResetStaleProcessingMessagesAsync(DateTime threshold, CancellationToken cancellationToken = default);
}
