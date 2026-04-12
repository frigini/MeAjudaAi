using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Contracts.Shared;
using Microsoft.EntityFrameworkCore;
using OutboxMessage = MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;

internal sealed class OutboxMessageRepository(CommunicationsDbContext context) : IOutboxMessageRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await context.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize = 20,
        DateTime? utcNow = null,
        CancellationToken cancellationToken = default)
    {
        var now = utcNow ?? DateTime.UtcNow;
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var messages = await context.OutboxMessages
                    .Where(x => x.Status == EOutboxMessageStatus.Pending && (x.ScheduledAt == null || x.ScheduledAt <= now))
                    .OrderByDescending(x => x.Priority)
                    .ThenBy(x => x.CreatedAt)
                    .Take(batchSize)
                    .ToListAsync(cancellationToken);

                if (messages.Count != 0)
                {
                    foreach (var message in messages)
                    {
                        message.MarkAsProcessing();
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return messages;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<int> CountByStatusAsync(EOutboxMessageStatus status, CancellationToken cancellationToken = default)
    {
        return await context.OutboxMessages.CountAsync(x => x.Status == status, cancellationToken);
    }

    public async Task<int> CleanupOldMessagesAsync(DateTime threshold, CancellationToken cancellationToken = default)
    {
        var oldMessages = await context.OutboxMessages
            .Where(x => x.Status == EOutboxMessageStatus.Sent && x.CreatedAt < threshold)
            .ToListAsync(cancellationToken);

        if (oldMessages.Count == 0) return 0;

        context.OutboxMessages.RemoveRange(oldMessages);
        await context.SaveChangesAsync(cancellationToken);
        return oldMessages.Count;
    }

    public async Task<int> ResetStaleProcessingMessagesAsync(DateTime threshold, CancellationToken cancellationToken = default)
    {
        var staleMessages = await context.OutboxMessages
            .Where(x => x.Status == EOutboxMessageStatus.Processing && (x.UpdatedAt ?? x.CreatedAt) < threshold)
            .ToListAsync(cancellationToken);

        if (staleMessages.Count == 0) return 0;

        foreach (var message in staleMessages)
        {
            message.ResetToPending();
        }

        await context.SaveChangesAsync(cancellationToken);
        return staleMessages.Count;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
