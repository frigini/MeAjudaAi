using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Queries;

/// <summary>
/// Implementação concreta da leitura pura (NoTracking) para prestadores de serviços.
/// </summary>
public sealed class DbContextProviderQueries(ProvidersDbContext context) : IProviderQueries
{
    private readonly ProvidersDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.CanConnectAsync(cancellationToken);
    }

    private IQueryable<Provider> GetProvidersQuery()
    {
        return _context.Providers
            .AsNoTracking()
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Include(p => p.Services);
    }

    /// <inheritdoc />
    public async Task<Provider?> GetByIdAsync(ProviderId id, CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Provider?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Provider>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null || ids.Count == 0)
            return Array.Empty<Provider>();

        var idSet = new HashSet<Guid>(ids);
        var allProviders = await GetProvidersQuery()
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);

        return allProviders
            .Where(p => idSet.Contains(p.Id.Value))
            .OrderBy(p => p.Id.Value)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Provider?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .FirstOrDefaultAsync(p => p.Documents.Any(d => d.Number == document) && !p.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Providers
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && !p.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Provider>> GetByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        var escapedCity = EscapeLikePattern(city);
        var likePattern = $"%{escapedCity}%";

        var allProviders = await GetProvidersQuery()
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);

        return allProviders
            .Where(p => p.BusinessProfile?.PrimaryAddress != null)
            .Where(p => LikeMatch(p.BusinessProfile!.PrimaryAddress!.City, likePattern))
            .OrderBy(p => p.Id.Value)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Provider>> GetByStateAsync(string state, CancellationToken cancellationToken = default)
    {
        var escapedState = EscapeLikePattern(state);
        var likePattern = $"%{escapedState}%";

        var allProviders = await GetProvidersQuery()
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);

        return allProviders
            .Where(p => p.BusinessProfile?.PrimaryAddress != null)
            .Where(p => LikeMatch(p.BusinessProfile!.PrimaryAddress!.State, likePattern))
            .OrderBy(p => p.Id.Value)
            .ToList();
    }

    private static bool LikeMatch(string value, string pattern)
    {
        if (value == null) return false;
        var escaped = pattern.Replace("\\%", "%").Replace("\\_", "_");
        return value.Contains(escaped.Trim('%'), StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Provider>> GetByVerificationStatusAsync(
        EVerificationStatus verificationStatus,
        CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .Where(p => !p.IsDeleted)
            .Where(p => p.VerificationStatus == verificationStatus)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Provider>> GetByTypeAsync(
        EProviderType type,
        CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .Where(p => !p.IsDeleted)
            .Where(p => p.Type == type)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(bool Exists, EVerificationStatus? Status)> GetProviderStatusAsync(
        ProviderId id,
        CancellationToken cancellationToken = default)
    {
        var result = await _context.Providers
            .AsNoTracking()
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => new { p.VerificationStatus })
            .FirstOrDefaultAsync(cancellationToken);

        return result is null
            ? (false, null)
            : (true, result.VerificationStatus);
    }

    /// <inheritdoc />
    public async Task<bool> HasProvidersWithServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await _context.Providers
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .AnyAsync(p => p.Services.Any(s => s.ServiceId == serviceId), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Provider>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .Where(p => !p.IsDeleted && p.Services.Any(s => s.ServiceId == serviceId))
            .OrderBy(p => p.Id.Value)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedResult<Provider>> GetPagedAsync(
        int page,
        int pageSize,
        string? nameFilter = null,
        EProviderType? typeFilter = null,
        EVerificationStatus? verificationStatusFilter = null,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

        var baseQuery = GetProvidersQuery().Where(p => !p.IsDeleted);

        if (typeFilter.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.Type == typeFilter.Value);
        }

        if (verificationStatusFilter.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.VerificationStatus == verificationStatusFilter.Value);
        }

        var allProviders = await baseQuery
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var escapedFilter = EscapeLikePattern(nameFilter);
            var likePattern = $"%{escapedFilter}%";
            allProviders = allProviders
                .Where(p => LikeMatch(p.Name, likePattern))
                .ToList();
        }

        var totalCount = allProviders.Count;

        var pagedProviders = allProviders
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<Provider>
        {
            Items = pagedProviders,
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalCount
        };
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(ProviderId id, CancellationToken cancellationToken = default)
    {
        return await _context.Providers
            .AsNoTracking()
            .AnyAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    private static string EscapeLikePattern(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
}
