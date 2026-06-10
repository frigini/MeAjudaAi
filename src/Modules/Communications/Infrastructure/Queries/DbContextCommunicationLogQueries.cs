using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
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
        maxResults = Math.Clamp(maxResults, 1, 100);
        return await dbContext.CommunicationLogs
            .AsNoTracking()
            .Where(x => x.Recipient == recipient)
            .OrderByDescending(x => x.CreatedAt)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<CommunicationLog> Items, int TotalCount)> SearchAsync(
        CommunicationLogQuery queryParams, CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, queryParams.PageNumber);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);

        var query = dbContext.CommunicationLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(queryParams.CorrelationId))
            query = query.Where(x => x.CorrelationId.Contains(queryParams.CorrelationId));
        if (!string.IsNullOrWhiteSpace(queryParams.Channel))
        {
            if (Enum.TryParse<ECommunicationChannel>(queryParams.Channel, true, out var ch))
                query = query.Where(x => x.Channel == ch);
            else
                query = query.Where(x => false);
        }
        if (!string.IsNullOrWhiteSpace(queryParams.Recipient))
            query = query.Where(x => x.Recipient.Contains(queryParams.Recipient));
        if (queryParams.IsSuccess.HasValue)
            query = query.Where(x => x.IsSuccess == queryParams.IsSuccess.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
