using MeAjudaAi.Shared.Database.Abstractions;
using System.Reflection;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados para o módulo ServiceCatalogs.
/// Implementa IUnitOfWork e gerencia os agregados Service e ServiceCategory.
/// </summary>
public partial class ServiceCatalogsDbContext : BaseDbContext, IUnitOfWork
{
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<ServiceCategory> ServiceCategories { get; set; } = null!;

    /// <summary>
    /// Inicializa uma nova instância para operações design-time (migrações).
    /// </summary>
    public ServiceCatalogsDbContext(DbContextOptions<ServiceCatalogsDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância para runtime com injeção de dependência.
    /// </summary>
    public ServiceCatalogsDbContext(
        DbContextOptions<ServiceCatalogsDbContext> options,
        IDomainEventProcessor domainEventProcessor)
        : base(options, domainEventProcessor)
    {
    }

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
            return repository;

        throw new InvalidOperationException(
            $"ServiceCatalogsDbContext does not implement IRepository<{typeof(TAggregate).Name}, {typeof(TKey).Name}>. " +
            $"This context supports: Service(ServiceId), ServiceCategory(ServiceCategoryId).");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("service_catalogs");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
