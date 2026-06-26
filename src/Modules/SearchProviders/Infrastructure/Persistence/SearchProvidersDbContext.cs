using MeAjudaAi.Shared.Database.Abstractions;
using System.Reflection;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados para o módulo SearchProviders.
/// Usa a extensão PostGIS para consultas geoespaciais.
/// </summary>
public partial class SearchProvidersDbContext : BaseDbContext, IUnitOfWork
{
    public DbSet<SearchableProvider> SearchableProviders => Set<SearchableProvider>();

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
            return repository;

        throw new InvalidOperationException(
            $"SearchProvidersDbContext does not support repository for {typeof(TAggregate).Name} with key {typeof(TKey).Name}. " +
            $"Supported: SearchableProvider(SearchableProviderId).");
    }

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
        modelBuilder.HasDefaultSchema(Schemas.SearchProviders);

        // Habilita extensão PostGIS para recursos geoespaciais
        modelBuilder.HasPostgresExtension("postgis");

        // Aplica configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}



