using System.Reflection;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
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
public partial class ServiceCatalogsDbContext : BaseDbContext, IUnitOfWork
{
    public ServiceCatalogsDbContext(DbContextOptions<ServiceCatalogsDbContext> options) 
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

    protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        return Task.FromResult(domainEvents);
    }

    protected override void ClearDomainEvents()
    {
        var entities = ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity);

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}

public partial class ServiceCatalogsDbContext : 
    IRepository<ServiceCategory, ServiceCategoryId>,
    IRepository<Service, ServiceId>,
    IServiceCategoryRepository,
    IServiceRepository
{
    // Implementation of IServiceCategoryRepository
    async Task<ServiceCategory?> IServiceCategoryRepository.GetByIdAsync(ServiceCategoryId id, CancellationToken ct) =>
        await ServiceCategories.FirstOrDefaultAsync(x => x.Id == id, ct);

    async Task<ServiceCategory?> IServiceCategoryRepository.GetByNameAsync(string name, CancellationToken ct) =>
        await ServiceCategories.FirstOrDefaultAsync(x => x.Name == name, ct);

    async Task<IReadOnlyList<ServiceCategory>> IServiceCategoryRepository.GetAllAsync(bool activeOnly, CancellationToken ct)
    {
        var query = ServiceCategories.AsQueryable();
        if (activeOnly) query = query.Where(x => x.IsActive);
        return await query.ToListAsync(ct);
    }

    async Task<bool> IServiceCategoryRepository.ExistsWithNameAsync(string name, ServiceCategoryId? excludeId, CancellationToken ct)
    {
        var query = ServiceCategories.Where(x => x.Name == name);
        if (excludeId is not null) query = query.Where(x => x.Id != excludeId);
        return await query.AnyAsync(ct);
    }

    Task IServiceCategoryRepository.AddAsync(ServiceCategory category, CancellationToken ct)
    {
        ServiceCategories.Add(category);
        return Task.CompletedTask;
    }

    Task IServiceCategoryRepository.UpdateAsync(ServiceCategory category, CancellationToken ct)
    {
        ServiceCategories.Update(category);
        return Task.CompletedTask;
    }

    Task IServiceCategoryRepository.DeleteAsync(ServiceCategoryId id, CancellationToken ct)
    {
        var category = ServiceCategories.Find(id);
        if (category != null) ServiceCategories.Remove(category);
        return Task.CompletedTask;
    }

    // Implementation of IServiceRepository
    async Task<Service?> IServiceRepository.GetByIdAsync(ServiceId id, CancellationToken ct) =>
        await Services.FirstOrDefaultAsync(x => x.Id == id, ct);

    async Task<IReadOnlyList<Service>> IServiceRepository.GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken ct) =>
        await Services.Where(x => ids.Contains(x.Id)).ToListAsync(ct);

    async Task<Service?> IServiceRepository.GetByNameAsync(string name, CancellationToken ct) =>
        await Services.FirstOrDefaultAsync(x => x.Name == name, ct);

    async Task<IReadOnlyList<Service>> IServiceRepository.GetAllAsync(bool activeOnly, CancellationToken ct)
    {
        var query = Services.AsQueryable();
        if (activeOnly) query = query.Where(x => x.IsActive);
        return await query.ToListAsync(ct);
    }

    async Task<IReadOnlyList<Service>> IServiceRepository.GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly, CancellationToken ct)
    {
        var query = Services.Where(x => x.CategoryId == categoryId);
        if (activeOnly) query = query.Where(x => x.IsActive);
        return await query.ToListAsync(ct);
    }

    async Task<bool> IServiceRepository.ExistsWithNameAsync(string name, ServiceId? excludeId, ServiceCategoryId? categoryId, CancellationToken ct)
    {
        var query = Services.Where(x => x.Name == name);
        if (excludeId is not null) query = query.Where(x => x.Id != excludeId);
        if (categoryId is not null) query = query.Where(x => x.CategoryId == categoryId);
        return await query.AnyAsync(ct);
    }

    async Task<int> IServiceRepository.CountByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly, CancellationToken ct)
    {
        var query = Services.Where(x => x.CategoryId == categoryId);
        if (activeOnly) query = query.Where(x => x.IsActive);
        return await query.CountAsync(ct);
    }

    Task IServiceRepository.AddAsync(Service service, CancellationToken ct)
    {
        Services.Add(service);
        return Task.CompletedTask;
    }

    Task IServiceRepository.UpdateAsync(Service service, CancellationToken ct)
    {
        Services.Update(service);
        return Task.CompletedTask;
    }

    Task IServiceRepository.DeleteAsync(ServiceId id, CancellationToken ct)
    {
        var service = Services.Find(id);
        if (service != null) Services.Remove(service);
        return Task.CompletedTask;
    }

    // Existing repository implementation
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
