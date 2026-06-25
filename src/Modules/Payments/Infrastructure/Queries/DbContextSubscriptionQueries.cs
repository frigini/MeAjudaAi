using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Queries;

public class DbContextSubscriptionQueries(PaymentsDbContext _dbContext) : ISubscriptionQueries
{
    private readonly PaymentsDbContext _dbContext = _dbContext ?? throw new ArgumentNullException(nameof(_dbContext));

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscriptions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Subscription?> GetActiveByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscriptions.AsNoTracking()
            .Where(s => s.ProviderId == providerId && s.Status == ESubscriptionStatus.Active)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetLatestByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscriptions
            .Where(s => s.ProviderId == providerId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByExternalIdAsync(string externalSubscriptionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalSubscriptionId))
            throw new ArgumentException("ExternalSubscriptionId cannot be null or empty.", nameof(externalSubscriptionId));
        return await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == externalSubscriptionId, cancellationToken);
    }
}
