using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

/// <summary>
/// Implementação de idempotência específica para o módulo de Providers.
/// </summary>
internal sealed class ProviderIdempotencyRepository(ProvidersDbContext context) : IIdempotencyRepository
{
    public async Task<bool> IsProcessedAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        return await context.Set<ProcessedIntegrationEvent>()
            .AnyAsync(e => e.CorrelationId == correlationId, cancellationToken);
    }

    public async Task MarkAsProcessedAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        await context.Set<ProcessedIntegrationEvent>().AddAsync(new ProcessedIntegrationEvent(correlationId, DateTime.UtcNow), cancellationToken);
    }
}
