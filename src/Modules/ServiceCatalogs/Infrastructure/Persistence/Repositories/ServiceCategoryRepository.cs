using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.Repositories;

public sealed class ServiceCategoryRepository : IServiceCategoryRepository
{
    private readonly ServiceCatalogsDbContext _context;

    public ServiceCategoryRepository(ServiceCatalogsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ServiceCategory?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ServiceCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalizedName = name.Trim().ToLower();
        return await _context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceCategory>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ServiceCategories.AsQueryable();

        if (activeOnly)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var normalizedName = name.Trim().ToLower();
        var query = _context.ServiceCategories.Where(c => c.Name.ToLower() == normalizedName);

        if (excludeId is not null)
            query = query.Where(c => c.Id != excludeId);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(ServiceCategory category, CancellationToken cancellationToken = default)
    {
        await _context.ServiceCategories.AddAsync(category, cancellationToken);
    }

    public async Task UpdateAsync(ServiceCategory category, CancellationToken cancellationToken = default)
    {
        _context.ServiceCategories.Update(category);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(ServiceCategoryId id, CancellationToken cancellationToken = default)
    {
        var category = await GetByIdAsync(id, cancellationToken);
        if (category is not null)
        {
            _context.ServiceCategories.Remove(category);
        }
    }
}