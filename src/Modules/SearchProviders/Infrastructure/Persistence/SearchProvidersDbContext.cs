using System.Reflection;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados para o módulo SearchProviders.
/// Usa a extensão PostGIS para consultas geoespaciais.
/// </summary>
public class SearchProvidersDbContext : BaseDbContext
{
    public DbSet<SearchableProvider> SearchableProviders => Set<SearchableProvider>();

    // Construtor para design-time (migrations)
    public SearchProvidersDbContext(DbContextOptions<SearchProvidersDbContext> options) : base(options)
    {
    }

    // Construtor para runtime com DI
    public SearchProvidersDbContext(DbContextOptions<SearchProvidersDbContext> options, IDomainEventProcessor domainEventProcessor)
        : base(options, domainEventProcessor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("meajudaai_searchproviders");

        // Habilita extensão PostGIS para recursos geoespaciais
        modelBuilder.HasPostgresExtension("postgis");

        // Aplica configurações do assembly
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
