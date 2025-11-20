using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.Repositories;

public sealed class ServiceRepository(ServiceCatalogsDbContext context) : IServiceRepository
{
    public async Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default)
    {
        return await context.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken cancellationToken = default)
    {
        // Guard against null or empty to prevent NullReferenceException
        if (ids == null)
            return Array.Empty<Service>();
        
        var idList = ids.ToList();
        if (idList.Count == 0)
            return Array.Empty<Service>();
        
        return await context.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .Where(s => idList.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Service?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name?.Trim() ?? string.Empty;

        return await context.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Name == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Service> query = context.Services
            .AsNoTracking()
            .Include(s => s.Category);

        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = context.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .Where(s => s.CategoryId == categoryId);

        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId = null, ServiceCategoryId? categoryId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name?.Trim() ?? string.Empty;
        var query = context.Services.Where(s => s.Name == normalized);

        if (excludeId is not null)
            query = query.Where(s => s.Id != excludeId);

        if (categoryId is not null)
            query = query.Where(s => s.CategoryId == categoryId);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = context.Services.Where(s => s.CategoryId == categoryId);

        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query.CountAsync(cancellationToken);
    }

    // NOTE: Write methods call SaveChangesAsync directly, treating each operation as a unit of work.
    // This is appropriate for single-aggregate commands. If multi-aggregate transactions are needed
    // in the future, consider introducing a shared unit-of-work abstraction.

    public async Task AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        await context.Services.AddAsync(service, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Service service, CancellationToken cancellationToken = default)
    {
        // Attach and mark as modified to ensure EF tracks changes
        var entry = context.Entry(service);
        if (entry.State == EntityState.Detached)
        {
            context.Services.Attach(service);
            entry.State = EntityState.Modified;
        }
        else
        {
            context.Services.Update(service);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ServiceId id, CancellationToken cancellationToken = default)
    {
        // Use lightweight lookup without includes for delete
        var service = await context.Services
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (service is not null)
        {
            context.Services.Remove(service);
            await context.SaveChangesAsync(cancellationToken);
        }
        // Delete is idempotent - no-op if service doesn't exist
    }
}
