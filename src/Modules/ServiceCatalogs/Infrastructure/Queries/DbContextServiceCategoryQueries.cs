using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;

public class DbContextServiceCategoryQueries(ServiceCatalogsDbContext dbContext) : IServiceCategoryQueries
{
    public async Task<ServiceCategory?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default) =>
        await dbContext.ServiceCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ServiceCategory>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ServiceCategories.AsNoTracking();

        if (activeOnly)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId, CancellationToken cancellationToken = default)
    {
        var normalized = name?.Trim() ?? string.Empty;
        var query = dbContext.ServiceCategories.AsNoTracking().Where(c => c.Name == normalized);

        if (excludeId is not null)
            query = query.Where(c => c.Id != excludeId);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(ServiceCategory Category, int ServiceCount)>> GetAllWithServiceCountAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ServiceCategories.AsNoTracking();

        if (activeOnly)
            query = query.Where(c => c.IsActive);

        var result = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new
            {
                Category = c,
                ServiceCount = dbContext.Services.Count(s => s.CategoryId == c.Id && (!activeOnly || s.IsActive))
            })
            .ToListAsync(cancellationToken);

        return result.Select(r => (r.Category, r.ServiceCount)).ToList();
    }
}
