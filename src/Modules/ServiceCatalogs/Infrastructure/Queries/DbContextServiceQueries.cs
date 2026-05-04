using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;

public class DbContextServiceQueries(ServiceCatalogsDbContext dbContext) : IServiceQueries
{
    public async Task<IReadOnlyList<Service>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Services.AsQueryable();
        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query
            .OrderBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default)
        => await dbContext.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Service?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await dbContext.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);

    public async Task<IReadOnlyList<Service>> GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken cancellationToken = default)
        => await dbContext.Services
            .Where(s => ids.Contains(s.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Service>> GetByCategoryAsync(
        ServiceCategoryId categoryId,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Services.Where(s => s.CategoryId == categoryId);
        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query
            .OrderBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Services.Where(s => s.CategoryId == categoryId);
        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query.AsNoTracking().CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId, ServiceCategoryId? categoryId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Services.Where(s => s.Name == name);
        
        if (excludeId is not null)
            query = query.Where(s => s.Id != excludeId);
            
        if (categoryId is not null)
            query = query.Where(s => s.CategoryId == categoryId);

        return await query.AsNoTracking().AnyAsync(cancellationToken);
    }
}