using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;

public class DbContextServiceCategoryQueries(ServiceCatalogsDbContext dbContext) : IServiceCategoryQueries
{
    public async Task<IReadOnlyList<ServiceCategory>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ServiceCategories.AsQueryable();
        if (activeOnly)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceCategory?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default)
        => await dbContext.ServiceCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<ServiceCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await dbContext.ServiceCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

    public async Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ServiceCategories.Where(c => c.Name == name);
        if (excludeId is not null)
            query = query.Where(c => c.Id != excludeId);

        return await query.AsNoTracking().AnyAsync(cancellationToken);
    }
}