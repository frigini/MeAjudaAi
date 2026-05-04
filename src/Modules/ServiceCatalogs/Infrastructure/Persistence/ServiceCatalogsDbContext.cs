using System.Linq;
using System.Reflection;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados para o módulo ServiceCatalogs.
/// Gerencia entidades de catálogo de serviços e sua persistência.
/// </summary>
public partial class ServiceCatalogsDbContext : BaseDbContext, IServiceCatalogUnitOfWork
{
    internal ServiceCatalogsDbContext(DbContextOptions<ServiceCatalogsDbContext> options) 
        : base(options)
    {
    }

    public ServiceCatalogsDbContext(
        DbContextOptions<ServiceCatalogsDbContext> options, 
        IDomainEventProcessor domainEventProcessor) 
        : base(options, domainEventProcessor)
    {
    }

    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<MeAjudaAi.Shared.Database.Outbox.OutboxMessage> OutboxMessages => Set<MeAjudaAi.Shared.Database.Outbox.OutboxMessage>();

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
            return repository;
        
        throw new InvalidOperationException(
            $"ServiceCatalogsDbContext does not implement IRepository<{typeof(TAggregate).Name}, {typeof(TKey).Name}>. " +
            $"This context only supports: ServiceCategory(ServiceCategoryId), Service(ServiceId)");
    }

    public MeAjudaAi.Shared.Database.Outbox.IOutboxRepository<MeAjudaAi.Shared.Database.Outbox.OutboxMessage> GetOutboxRepository()
    {
        return new MeAjudaAi.Shared.Database.Outbox.OutboxRepository<MeAjudaAi.Shared.Database.Outbox.OutboxMessage>(this);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("service_catalogs");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }


}

public partial class ServiceCatalogsDbContext : 
    IRepository<ServiceCategory, ServiceCategoryId>,
    IRepository<Service, ServiceId>
{
    async Task<ServiceCategory?> IRepository<ServiceCategory, ServiceCategoryId>.TryFindAsync(ServiceCategoryId key, CancellationToken ct) =>
        await ServiceCategories.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<ServiceCategory, ServiceCategoryId>.Add(ServiceCategory aggregate) =>
        ServiceCategories.Add(aggregate);

    void IRepository<ServiceCategory, ServiceCategoryId>.Delete(ServiceCategory aggregate) =>
        ServiceCategories.Remove(aggregate);

    async Task<Service?> IRepository<Service, ServiceId>.TryFindAsync(ServiceId key, CancellationToken ct) =>
        await Services.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<Service, ServiceId>.Add(Service aggregate) =>
        Services.Add(aggregate);

    void IRepository<Service, ServiceId>.Delete(Service aggregate) =>
        Services.Remove(aggregate);
}
