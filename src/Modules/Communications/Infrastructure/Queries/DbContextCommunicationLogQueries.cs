using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Queries;

public class DbContextCommunicationLogQueries(CommunicationsDbContext dbContext) : ICommunicationLogQueries
{
    public async Task<bool> ExistsByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
        => await dbContext.CommunicationLogs.AsNoTracking().AnyAsync(x => x.CorrelationId == correlationId, cancellationToken);

    public async Task<IReadOnlyList<CommunicationLog>> GetByRecipientAsync(
        string recipient, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        return await dbContext.CommunicationLogs
            .AsNoTracking()
            .Where(x => x.Recipient == recipient)
            .OrderByDescending(x => x.CreatedAt)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<CommunicationLog> Items, int TotalCount)> SearchAsync(
        string? correlationId = null, string? channel = null, string? recipient = null,
        bool? isSuccess = null, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = dbContext.CommunicationLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(correlationId))
            query = query.Where(x => x.CorrelationId.Contains(correlationId));
        if (!string.IsNullOrWhiteSpace(channel))
        {
            if (Enum.TryParse<ECommunicationChannel>(channel, true, out var ch))
                query = query.Where(x => x.Channel == ch);
            else
                query = query.Where(x => false);
        }
        if (!string.IsNullOrWhiteSpace(recipient))
            query = query.Where(x => x.Recipient.Contains(recipient));
        if (isSuccess.HasValue)
            query = query.Where(x => x.IsSuccess == isSuccess.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
