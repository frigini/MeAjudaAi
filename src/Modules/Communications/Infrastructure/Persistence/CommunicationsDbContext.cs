using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence;

public partial class CommunicationsDbContext : BaseDbContext, IUnitOfWork
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<CommunicationLog> CommunicationLogs => Set<CommunicationLog>();

    public CommunicationsDbContext(DbContextOptions<CommunicationsDbContext> options) : base(options)
    {
    }

    public CommunicationsDbContext(DbContextOptions<CommunicationsDbContext> options, IDomainEventProcessor domainEventProcessor) 
        : base(options, domainEventProcessor)
    {
    }

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
            return repository;

        throw new InvalidOperationException(
            $"CommunicationsDbContext does not implement IRepository<{typeof(TAggregate).Name}, {typeof(TKey).Name}>. " +
            $"This context supports: EmailTemplate(Guid), CommunicationLog(Guid).");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("communications");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommunicationsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(x => x.Entity)
            .SelectMany(x => x.DomainEvents)
            .ToList();

        return Task.FromResult(domainEvents);
    }

    protected override void ClearDomainEvents()
    {
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Select(x => x.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}



