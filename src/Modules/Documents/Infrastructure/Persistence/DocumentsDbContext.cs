using System.Reflection;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados para o módulo de Documentos.
/// Gerencia entidades de documentos e sua persistência.
/// </summary>
public partial class DocumentsDbContext : BaseDbContext, IUnitOfWork
{
    /// <summary>
    /// Obtém a coleção de documentos.
    /// </summary>
    public DbSet<Document> Documents => Set<Document>();

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
            return repository;

        throw new InvalidOperationException(
            $"DocumentsDbContext does not implement IRepository<{typeof(TAggregate).Name}, {typeof(TKey).Name}>. " +
            $"This context only supports: Document(DocumentId)");
    }


    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DocumentsDbContext"/> para operações design-time (migrações).
    /// </summary>
    /// <param name="options">As opções a serem usadas pelo DbContext.</param>
    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DocumentsDbContext"/> para runtime com injeção de dependência.
    /// </summary>
    /// <param name="options">As opções a serem usadas pelo DbContext.</param>
    /// <param name="domainEventProcessor">O processador de eventos de domínio.</param>
    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options, IDomainEventProcessor domainEventProcessor) : base(options, domainEventProcessor)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("documents");

        // Aplica configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Ignora OutboxMessage explicitamente para evitar erro de migração em E2E
        modelBuilder.Ignore<MeAjudaAi.Shared.Database.Outbox.OutboxMessage>();

        base.OnModelCreating(modelBuilder);
    }

    protected override async Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries()
            .Where(entry => entry.Entity is AggregateRoot<Guid>)
            .SelectMany(entry => ((AggregateRoot<Guid>)entry.Entity).DomainEvents)
            .ToList();

        return await Task.FromResult(domainEvents);
    }

    protected override void ClearDomainEvents()
    {
        var entities = ChangeTracker
            .Entries()
            .Where(entry => entry.Entity is AggregateRoot<Guid>)
            .Select(entry => (AggregateRoot<Guid>)entry.Entity);

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}
