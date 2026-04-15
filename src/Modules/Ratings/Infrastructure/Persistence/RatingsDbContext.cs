using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public class RatingsDbContext : BaseDbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ratings");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot<ReviewId>>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        return Task.FromResult(domainEvents);
    }

    protected override void ClearDomainEvents()
    {
        var entities = ChangeTracker
            .Entries<AggregateRoot<ReviewId>>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity);

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}
