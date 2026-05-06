using Microsoft.EntityFrameworkCore;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Contracts.Shared;

namespace MeAjudaAi.Shared.Database.Outbox;

public class OutboxRepository<TMessage>(DbContext dbContext) : IOutboxRepository<TMessage> 
    where TMessage : OutboxMessage
{
    private readonly DbSet<TMessage> _dbSet = dbContext.Set<TMessage>();

    public Task AddAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(message, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<TMessage>> GetPendingAsync(int batchSize = 20, DateTime? utcNow = null, CancellationToken cancellationToken = default)
    {
        var now = utcNow ?? DateTime.UtcNow;
        
        return await _dbSet
            .Where(m => m.Status == EOutboxMessageStatus.Pending)
            .Where(m => m.ScheduledAt == null || m.ScheduledAt <= now)
            .OrderByDescending(m => m.Priority)
            .ThenBy(m => m.ScheduledAt)
            .ThenBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
