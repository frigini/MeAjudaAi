using System.Reflection;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
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

    public DbSet<MeAjudaAi.Shared.Database.Outbox.OutboxMessage> OutboxMessages => Set<MeAjudaAi.Shared.Database.Outbox.OutboxMessage>();

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        var diagPath = @"C:\Code\MeAjudaAi\tests\MeAjudaAi.E2E.Tests\bin\Debug\net10.0\db_diag.log";
        System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [DB] GetRepository<{typeof(TAggregate).Name}> called...{System.Environment.NewLine}");
        if (this is IRepository<TAggregate, TKey> repository)
        {
            System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [DB] GetRepository<{typeof(TAggregate).Name}> returning this...{System.Environment.NewLine}");
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
