using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence.Repositories;

public sealed class ServiceRepository(CatalogsDbContext context) : IServiceRepository
{
    public async Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default)
    {
        return await context.Services
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Service?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Services
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = context.Services.AsQueryable();

        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = context.Services
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
        var query = context.Services.Where(s => s.Name == name);

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
        context.Services.Update(service);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ServiceId id, CancellationToken cancellationToken = default)
    {
        // Reuse GetByIdAsync but note it's a tracked query for delete scenarios
        var service = await GetByIdAsync(id, cancellationToken);
        if (service is not null)
        {
            context.Services.Remove(service);
            await context.SaveChangesAsync(cancellationToken);
        }
        // Delete is idempotent - no-op if service doesn't exist
    }
}
