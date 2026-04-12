namespace MeAjudaAi.Shared.Database.Outbox;

/// <summary>
/// Interface base para repositórios de Outbox.
/// </summary>
/// <typeparam name="TMessage">Tipo da entidade de mensagem do outbox.</typeparam>
public interface IOutboxRepository<TMessage> where TMessage : OutboxMessage
{
    /// <summary>
    /// Adiciona uma nova mensagem ao outbox.
    /// </summary>
    Task AddAsync(TMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera mensagens pendentes para processamento.
    /// </summary>
    Task<IReadOnlyList<TMessage>> GetPendingAsync(
        int batchSize = 20,
        DateTime? utcNow = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste as alterações no banco de dados.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
