using System.Reflection;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

/// <summary>
/// Contexto do Entity Framework para o módulo Providers.
/// </summary>
/// <remarks>
/// Implementa o padrão DbContext do Entity Framework Core para persistência
/// das entidades do domínio de prestadores de serviços.
/// </remarks>
public partial class ProvidersDbContext : BaseDbContext, IUnitOfWork
{
    /// <summary>
    /// Inicializa uma nova instância do contexto para design-time (migrations).
    /// </summary>
    public ProvidersDbContext(DbContextOptions<ProvidersDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância do contexto com suporte a eventos de domínio.
    /// </summary>
    public ProvidersDbContext(DbContextOptions<ProvidersDbContext> options, IDomainEventProcessor domainEventProcessor) 
        : base(options, domainEventProcessor)
    {
    }

    /// <summary>
    /// Conjunto de dados para prestadores de serviços.
    /// </summary>
    public DbSet<Provider> Providers => Set<Provider>();

    /// <summary>
    /// Obtém o repositório tipado para um agregado do domínio.
    /// </summary>
    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>() =>
        (IRepository<TAggregate, TKey>)this;

    /// <summary>
    /// Configura o modelo de dados durante a criação do contexto.
    /// </summary>
    /// <param name="modelBuilder">Construtor do modelo</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("providers");

        // Aplica configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<Provider>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        return Task.FromResult(domainEvents);
    }

    protected override void ClearDomainEvents()
    {
        var entries = ChangeTracker
            .Entries<Provider>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity);

        foreach (var entity in entries)
        {
            entity.ClearDomainEvents();
        }
    }
}
