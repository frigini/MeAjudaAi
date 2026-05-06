using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Queries;

public class DbContextReviewQueries(RatingsDbContext dbContext) : IReviewQueries
{
    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Review>> GetByProviderIdAsync(Guid providerId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProviderId == providerId && r.Status == EReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Review?> GetByProviderAndCustomerAsync(Guid providerId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ProviderId == providerId && r.CustomerId == customerId, cancellationToken);
    }

    public async Task<(decimal AverageRating, int TotalReviews)> GetAverageRatingForProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProviderId == providerId && r.Status == EReviewStatus.Approved);

        var total = await query.CountAsync(cancellationToken);

        if (total == 0)
            return (0, 0);

        var average = await query.AverageAsync(r => r.Rating, cancellationToken);
        return (Math.Round((decimal)average, 2), total);
    }
}
