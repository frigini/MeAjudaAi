using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Idempotency;

/// <summary>
/// Implementação de idempotência específica para o módulo de Ratings.
/// </summary>
internal sealed class RatingsIdempotencyRepository(RatingsDbContext context) : IIdempotencyRepository
{
    public async Task<bool> IsProcessedAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        var exists = await context.Database.SqlQueryRaw<bool>(
            "SELECT EXISTS(SELECT 1 FROM ratings.processed_integration_events WHERE correlation_id = {0})", 
            correlationId).FirstOrDefaultAsync(cancellationToken);
        return exists;
    }

    public async Task MarkAsProcessedAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO ratings.processed_integration_events (correlation_id, processed_at) VALUES ({0}, {1}) ON CONFLICT (correlation_id) DO NOTHING",
            correlationId, DateTime.UtcNow, cancellationToken);
    }
}
