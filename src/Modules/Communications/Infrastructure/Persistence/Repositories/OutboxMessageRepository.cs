using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;

internal sealed class OutboxMessageRepository(CommunicationsDbContext context) : IOutboxMessageRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await context.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return await context.OutboxMessages
            .Where(x => x.Status == EOutboxMessageStatus.Pending && (x.ScheduledAt == null || x.ScheduledAt <= utcNow))
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.OutboxMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<int> CountByStatusAsync(EOutboxMessageStatus status, CancellationToken cancellationToken = default)
    {
        return await context.OutboxMessages.CountAsync(x => x.Status == status, cancellationToken);
    }
}
