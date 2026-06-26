using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence;

public partial class PaymentsDbContext : BaseDbContext, IUnitOfWork
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
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
            return repository;

        throw new InvalidOperationException(
            $"PaymentsDbContext does not support repository for {typeof(TAggregate).Name} with key {typeof(TKey).Name}. " +
            $"Supported: Subscription(Guid), PaymentTransaction(Guid), InboxMessage(Guid).");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Payments);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}