using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados para o módulo Locations.
/// </summary>
public partial class LocationsDbContext : BaseDbContext, IUnitOfWork
{
    public DbSet<AllowedCity> AllowedCities => Set<AllowedCity>();

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
            return repository;

        throw new InvalidOperationException(
            $"LocationsDbContext does not support repository for {typeof(TAggregate).Name} with key {typeof(TKey).Name}. " +
            $"Supported: AllowedCity(Guid).");
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="LocationsDbContext"/> para operações design-time (migrações).
    /// </summary>
    /// <param name="options">As opções a serem usadas pelo DbContext.</param>
    public LocationsDbContext(DbContextOptions<LocationsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="LocationsDbContext"/> para runtime com injeção de dependência.
    /// </summary>
    /// <param name="options">As opções a serem usadas pelo DbContext.</param>
    /// <param name="domainEventProcessor">O processador de eventos de domínio.</param>
    public LocationsDbContext(DbContextOptions<LocationsDbContext> options, IDomainEventProcessor domainEventProcessor) : base(options, domainEventProcessor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Define schema padrão para este módulo
        modelBuilder.HasDefaultSchema(Schemas.Locations);

        // Aplica todas as configurações do assembly atual
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocationsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}