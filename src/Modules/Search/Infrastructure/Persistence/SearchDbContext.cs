using System.Reflection;
using MeAjudaAi.Modules.Search.Domain.Entities;
using MeAjudaAi.Modules.Search.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Search.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados para o módulo Search.
/// Usa a extensão PostGIS para consultas geoespaciais.
/// </summary>
public class SearchDbContext : BaseDbContext
{
    public DbSet<SearchableProvider> SearchableProviders => Set<SearchableProvider>();

    // Constructor for design-time (migrations)
    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options)
    {
    }

    // Constructor for runtime with DI
    public SearchDbContext(DbContextOptions<SearchDbContext> options, IDomainEventProcessor domainEventProcessor)
        : base(options, domainEventProcessor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("search");

        // Enable PostGIS extension for geospatial features
        modelBuilder.HasPostgresExtension("postgis");

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot<SearchableProviderId>>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        return Task.FromResult(domainEvents);
    }

    /// <summary>
    /// Limpa eventos de domínio após persistência.
    /// 
    /// NOTA: Específico para AggregateRoot&lt;SearchableProviderId&gt;.
    /// Se novos agregados com tipos de ID diferentes forem adicionados,
    /// considere usar interface base não-genérica (IAggregateRoot) para
    /// suportar múltiplos tipos sem modificações no DbContext.
    /// </summary>
    protected override void ClearDomainEvents()
    {
        var entities = ChangeTracker
            .Entries<AggregateRoot<SearchableProviderId>>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity);

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}
