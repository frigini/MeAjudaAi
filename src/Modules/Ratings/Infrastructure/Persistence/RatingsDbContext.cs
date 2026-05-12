using System.Reflection;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;

public partial class RatingsDbContext : BaseDbContext, IUnitOfWork
{
    private readonly ILogger<RatingsDbContext>? _logger;

    public RatingsDbContext(DbContextOptions<RatingsDbContext> options) 
        : base(options)
    {
    }

    public RatingsDbContext(
        DbContextOptions<RatingsDbContext> options, 
        IDomainEventProcessor domainEventProcessor,
        ILogger<RatingsDbContext>? logger = null) 
        : base(options, domainEventProcessor)
    {
        _logger = logger;
    }

    public DbSet<Review> Reviews { get; set; } = null!;

    public DbSet<MeAjudaAi.Shared.Database.Outbox.OutboxMessage> OutboxMessages => Set<MeAjudaAi.Shared.Database.Outbox.OutboxMessage>();

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        _logger?.LogDebug("GetRepository<{AggregateName}> called...", typeof(TAggregate).Name);
        if (this is IRepository<TAggregate, TKey> repository)
        {
            _logger?.LogDebug("GetRepository<{AggregateName}> returning this...", typeof(TAggregate).Name);
            return repository;
        }
        
        throw new InvalidOperationException(
            $"RatingsDbContext does not implement IRepository<{typeof(TAggregate).Name}, {typeof(TKey).Name}>. " +
            $"This context only supports: Review(ReviewId)");
    }


    public MeAjudaAi.Shared.Database.Outbox.IOutboxRepository<MeAjudaAi.Shared.Database.Outbox.OutboxMessage> GetOutboxRepository()
    {
        return new MeAjudaAi.Shared.Database.Outbox.OutboxRepository<MeAjudaAi.Shared.Database.Outbox.OutboxMessage>(this);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ratings");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
