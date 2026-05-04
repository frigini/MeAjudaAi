using System.Reflection;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
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

    /// <summary>
    /// Obtém a coleção de mensagens do outbox para este módulo.
    /// </summary>
    public DbSet<MeAjudaAi.Shared.Database.Outbox.OutboxMessage> OutboxMessages => Set<MeAjudaAi.Shared.Database.Outbox.OutboxMessage>();

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
            return repository;

        throw new InvalidOperationException(
            $"DocumentsDbContext does not implement IRepository<{typeof(TAggregate).Name}, {typeof(TKey).Name}>.");
    }

    public MeAjudaAi.Shared.Database.Outbox.IOutboxRepository<MeAjudaAi.Shared.Database.Outbox.OutboxMessage> GetOutboxRepository()
    {
        return new MeAjudaAi.Shared.Database.Outbox.OutboxRepository<MeAjudaAi.Shared.Database.Outbox.OutboxMessage>(this);
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

        base.OnModelCreating(modelBuilder);
    }


}
