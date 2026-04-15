using System.Reflection;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence;

public class PaymentsDbContext : BaseDbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) 
        : base(options)
    {
    }

    public PaymentsDbContext(
        DbContextOptions<PaymentsDbContext> options, 
        IDomainEventProcessor domainEventProcessor) 
        : base(options, domainEventProcessor)
    {
    }

    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<PaymentTransaction> Transactions { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("payments");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<BaseEntity>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        return Task.FromResult(domainEvents);
    }

    protected override void ClearDomainEvents()
    {
        var entities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity);

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}

public class InboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
}
