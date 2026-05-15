using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.Exceptions;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;

public partial class RatingsDbContext : IRepository<Review, ReviewId>
{
    async Task<Review?> IRepository<Review, ReviewId>.TryFindAsync(ReviewId key, CancellationToken ct) =>
        await Reviews.FirstOrDefaultAsync(r => r.Id == key, ct);

    void IRepository<Review, ReviewId>.Add(Review aggregate) =>
        Reviews.Add(aggregate);

    void IRepository<Review, ReviewId>.Delete(Review aggregate) =>
        Reviews.Remove(aggregate);
}

public class ReviewRepository(RatingsDbContext context) : IReviewRepository
{
    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Reviews.AddAsync(review, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            throw new DuplicateReviewException(review.ProviderId, review.CustomerId);
        }
    }

    public async Task UpdateAsync(Review review, CancellationToken cancellationToken = default)
    {
        context.Reviews.Update(review);
        await context.SaveChangesAsync(cancellationToken);
    }
}