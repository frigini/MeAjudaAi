using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;

public partial class RatingsDbContext : IRepository<Review, ReviewId>
{
    async Task<Review?> IRepository<Review, ReviewId>.TryFindAsync(ReviewId key, CancellationToken ct) =>
        await Reviews.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<Review, ReviewId>.Add(Review aggregate)
    {
        var diagPath = @"C:\Code\MeAjudaAi\tests\MeAjudaAi.E2E.Tests\bin\Debug\net10.0\db_diag.log";
        System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [DB] IRepository<Review>.Add starting for review {aggregate.Id.Value}...{System.Environment.NewLine}");
        Reviews.Add(aggregate);
        System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [DB] IRepository<Review>.Add completed.{System.Environment.NewLine}");
    }

    void IRepository<Review, ReviewId>.Delete(Review aggregate) =>
        Reviews.Remove(aggregate);
}
