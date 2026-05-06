using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;

public partial class RatingsDbContext : IRepository<Review, ReviewId>
{
    async Task<Review?> IRepository<Review, ReviewId>.TryFindAsync(ReviewId key, CancellationToken ct) =>
        await Reviews.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<Review, ReviewId>.Add(Review aggregate) =>
        Reviews.Add(aggregate);

    void IRepository<Review, ReviewId>.Delete(Review aggregate) =>
        Reviews.Remove(aggregate);
}
