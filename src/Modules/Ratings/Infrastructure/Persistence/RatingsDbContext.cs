using MeAjudaAi.Shared.Database.Abstractions;
using System.Reflection;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;

public partial class RatingsDbContext : BaseDbContext, IUnitOfWork
{
    public RatingsDbContext(DbContextOptions<RatingsDbContext> options)
        : base(options)
    {
    }

    public RatingsDbContext(
        DbContextOptions<RatingsDbContext> options,
        IDomainEventProcessor domainEventProcessor)
        : base(options, domainEventProcessor)
    {
    }

    public DbSet<Review> Reviews { get; set; } = null!;

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
            return repository;

        throw new InvalidOperationException(
            $"RatingsDbContext does not support repository for {typeof(TAggregate).Name} with key {typeof(TKey).Name}. " +
            $"Supported: Review(ReviewId).");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Ratings);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}


