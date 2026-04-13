using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Repositories;

public class ReviewRepository(RatingsDbContext context) : IReviewRepository
{
    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        await context.Reviews.AddAsync(review, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Review?> GetByIdAsync(ReviewId id, CancellationToken cancellationToken = default)
    {
        return await context.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Review>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await context.Reviews
            .Where(r => r.ProviderId == providerId && r.Status == EReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Review review, CancellationToken cancellationToken = default)
    {
        context.Reviews.Update(review);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Review?> GetByProviderAndCustomerAsync(Guid providerId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await context.Reviews
            .FirstOrDefaultAsync(r => r.ProviderId == providerId && r.CustomerId == customerId, cancellationToken);
    }

    public async Task<(decimal AverageRating, int TotalReviews)> GetAverageRatingForProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var query = context.Reviews
            .Where(r => r.ProviderId == providerId && r.Status == EReviewStatus.Approved);

        var total = await query.CountAsync(cancellationToken);

        if (total == 0)
            return (0, 0);

        var average = await query.AverageAsync(r => r.Rating, cancellationToken);
        return (Math.Round((decimal)average, 2), total);
    }
}
