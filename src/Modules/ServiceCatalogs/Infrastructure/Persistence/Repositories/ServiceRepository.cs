using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.Repositories;

public sealed class ServiceRepository : IServiceRepository
{
    private readonly ServiceCatalogsDbContext _context;

    public ServiceRepository(ServiceCatalogsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids?.ToList();
        if (idList is null || idList.Count == 0)
            return Array.Empty<Service>();

        return await _context.Services
            .Include(s => s.Category)
            .Where(s => idList.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Service?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalizedName = name.Trim().ToLower();
        return await _context.Services
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Name.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Services.Include(s => s.Category).AsQueryable();

        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Services
            .Include(s => s.Category)
            .Where(s => s.CategoryId == categoryId);

        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var normalizedName = name.Trim().ToLower();
        var query = _context.Services.Where(s => s.Name.ToLower() == normalizedName);

        if (excludeId is not null)
            query = query.Where(s => s.Id != excludeId);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        await _context.Services.AddAsync(service, cancellationToken);
    }

    public async Task UpdateAsync(Service service, CancellationToken cancellationToken = default)
    {
        _context.Services.Update(service);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(ServiceId id, CancellationToken cancellationToken = default)
    {
        var service = await GetByIdAsync(id, cancellationToken);
        if (service is not null)
        {
            _context.Services.Remove(service);
        }
    }
}