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
        // Validação e normalização de parâmetros de paginação
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MeAjudaAi.Shared.Utilities.Constants.ValidationConstants.Pagination.MaxPageSize);

        var offset = (page - 1) * pageSize;

        return await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProviderId == providerId && r.Status == EReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
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
        var stats = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProviderId == providerId && r.Status == EReviewStatus.Approved)
            .GroupBy(r => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Average = g.Average(r => (double)r.Rating)
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (stats == null || stats.Total == 0)
            return (0, 0);

        return (Math.Round((decimal)stats.Average, 2), stats.Total);
    }
}
