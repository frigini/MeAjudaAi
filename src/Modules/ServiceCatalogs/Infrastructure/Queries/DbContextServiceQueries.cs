using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;

public class DbContextServiceQueries(ServiceCatalogsDbContext _dbContext) : IServiceQueries
{
    private readonly ServiceCatalogsDbContext _dbContext = _dbContext ?? throw new ArgumentNullException(nameof(_dbContext));

    public async Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default) =>
        await _dbContext.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Service>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Service> query = _dbContext.Services
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
        var query = _dbContext.Services
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

    public async Task<IReadOnlyList<Service>> GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null) return Array.Empty<Service>();
        var idList = ids.ToList();
        if (idList.Count == 0) return Array.Empty<Service>();

        return await _dbContext.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .Where(s => idList.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId, ServiceCategoryId? categoryId, CancellationToken cancellationToken = default)
    {
        var normalized = name?.Trim() ?? string.Empty;
        var query = _dbContext.Services.AsNoTracking().Where(s => s.Name == normalized);

        if (excludeId is not null)
            query = query.Where(s => s.Id != excludeId);

        if (categoryId is not null)
            query = query.Where(s => s.CategoryId == categoryId);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Services.AsNoTracking().Where(s => s.CategoryId == categoryId);

        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<ServiceCategoryId, (int Total, int Active)>> CountByCategoriesAsync(
        IEnumerable<ServiceCategoryId> categoryIds,
        CancellationToken cancellationToken = default)
    {
        var idList = categoryIds.ToList();
        if (idList.Count == 0)
            return new Dictionary<ServiceCategoryId, (int Total, int Active)>();

        var counts = await _dbContext.Services
            .AsNoTracking()
            .Where(s => idList.Contains(s.CategoryId))
            .GroupBy(s => s.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Total = g.Count(),
                Active = g.Count(s => s.IsActive)
            })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(
            c => c.CategoryId,
            c => (c.Total, c.Active));
    }
}
