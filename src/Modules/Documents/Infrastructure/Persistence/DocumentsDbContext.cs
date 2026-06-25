using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Persistence;

/// <summary>
/// Database context for the Documents module.
/// Manages document entities and their persistence.
/// </summary>
public partial class DocumentsDbContext : BaseDbContext, IUnitOfWork
{
    /// <summary>
    /// Gets the collection of documents.
    /// </summary>
    public DbSet<Document> Documents => Set<Document>();

    /// <summary>
    /// Obtém a coleção de mensagens de outbox.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

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
        modelBuilder.HasDefaultSchema(Schemas.Documents);

        // Aplica configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}