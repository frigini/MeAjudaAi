namespace MeAjudaAi.Shared.Database.Idempotency;

/// <summary>
/// Interface para verificar e marcar eventos de integração como processados.
/// </summary>
public interface IIdempotencyRepository
{
    Task<bool> IsProcessedAsync(string correlationId, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string correlationId, CancellationToken cancellationToken = default);
}
