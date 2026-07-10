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
            "SELECT EXISTS(SELECT 1 FROM ratings.processed_integration_events WHERE \"CorrelationId\" = {0}) AS \"Value\"", 
            correlationId).FirstOrDefaultAsync(cancellationToken);
        return exists;
    }

    public async Task MarkAsProcessedAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO ratings.processed_integration_events (\"CorrelationId\", \"ProcessedAt\") VALUES ({correlationId}, {DateTime.UtcNow}) ON CONFLICT (\"CorrelationId\") DO NOTHING",
            cancellationToken);
    }
}
