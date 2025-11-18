using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence.Repositories;

public sealed class ServiceCategoryRepository(CatalogsDbContext context) : IServiceCategoryRepository
{
    public async Task<ServiceCategory?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default)
    {
        return await context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ServiceCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceCategory>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = context.ServiceCategories.AsQueryable();

        if (activeOnly)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = context.ServiceCategories.Where(c => c.Name == name);

        if (excludeId is not null)
            query = query.Where(c => c.Id != excludeId);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(ServiceCategory category, CancellationToken cancellationToken = default)
    {
        await context.ServiceCategories.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ServiceCategory category, CancellationToken cancellationToken = default)
    {
        context.ServiceCategories.Update(category);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ServiceCategoryId id, CancellationToken cancellationToken = default)
    {
        var category = await GetByIdAsync(id, cancellationToken);
        if (category is not null)
        {
            context.ServiceCategories.Remove(category);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
