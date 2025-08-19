using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Enums;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValuleObjects;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

public class ServiceProviderRepository : IServiceProviderRepository
{
    private readonly UsersDbContext _context;

    public ServiceProviderRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceProvider?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
    }

    public async Task<ServiceProvider?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<ServiceProvider>> GetByTierAsync(EServiceProviderTier tier, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .Where(sp => sp.Tier == tier)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceProvider>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .Where(sp => sp.ServiceCategories.Contains(category))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceProvider>> GetVerifiedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .Where(sp => sp.IsVerified)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<ServiceProvider> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        EServiceProviderTier? tier = null,
        bool? isVerified = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ServiceProviders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(sp =>
                sp.CompanyName.Contains(searchTerm) ||
                (sp.Description != null && sp.Description.Contains(searchTerm)));
        }

        if (tier.HasValue)
        {
            query = query.Where(sp => sp.Tier == tier.Value);
        }

        if (isVerified.HasValue)
        {
            query = query.Where(sp => sp.IsVerified == isVerified.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(sp => sp.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(ServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await _context.ServiceProviders.AddAsync(serviceProvider, cancellationToken);
    }

    public async Task UpdateAsync(ServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        _context.ServiceProviders.Update(serviceProvider);
    }

    public async Task DeleteAsync(ServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        _context.ServiceProviders.Remove(serviceProvider);
    }

    public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .AnyAsync(sp => sp.Id == id, cancellationToken);
    }

    public async Task<int> CountByTierAsync(EServiceProviderTier tier, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .CountAsync(sp => sp.Tier == tier, cancellationToken);
    }
}