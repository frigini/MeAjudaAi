using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence;

/// <summary>
/// DbContext para o módulo de comunicações.
/// </summary>
public sealed class CommunicationsDbContext : BaseDbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("communications");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommunicationsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<IDomainEvent>());
    }

    protected override void ClearDomainEvents()
    {
        // No domain events yet
    }
}
