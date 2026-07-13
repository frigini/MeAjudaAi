using MeAjudaAi.Shared.Database.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

/// <summary>
/// Implementação de idempotência específica para o módulo de Providers.
/// </summary>
internal sealed class ProviderIdempotencyRepository(ProvidersDbContext context) : IIdempotencyRepository
{
    public async Task<bool> IsProcessedAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        var exists = await context.Database.SqlQueryRaw<bool>(
            "SELECT EXISTS(SELECT 1 FROM providers.processed_integration_events WHERE correlation_id = {0}) AS \"Value\"", 
            correlationId).FirstOrDefaultAsync(cancellationToken);
        return exists;
    }

    public async Task MarkAsProcessedAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO providers.processed_integration_events (correlation_id, processed_at) VALUES ({correlationId}, {DateTime.UtcNow}) ON CONFLICT (correlation_id) DO NOTHING",
            cancellationToken);
    }
}
