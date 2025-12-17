using System.Reflection;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Persistence;

/// <summary>
/// Database context for the Documents module.
/// Manages document entities and their persistence.
/// </summary>
public class DocumentsDbContext : BaseDbContext
{
    /// <summary>
    /// Gets the collection of documents.
    /// </summary>
    public DbSet<Document> Documents => Set<Document>();

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentsDbContext"/> class for design-time (migrations).
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentsDbContext"/> class for runtime with dependency injection.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    /// <param name="domainEventProcessor">The domain event processor for publishing events.</param>
    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options, IDomainEventProcessor domainEventProcessor) : base(options, domainEventProcessor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("meajudaai_documents");

        // Aplica configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override async Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        // Se mais agregados com eventos de domínio forem adicionados a este contexto,
        // considere generalizar esta query usando um tipo base comum (ex: IAggregateRoot)
        // para capturar eventos de todas as entidades automaticamente
        var domainEvents = ChangeTracker
            .Entries<Document>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        return await Task.FromResult(domainEvents);
    }

    protected override void ClearDomainEvents()
    {
        // Nota: Se mais agregados forem adicionados, generalize para capturar todos
        var entities = ChangeTracker
            .Entries<Document>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity);

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}
