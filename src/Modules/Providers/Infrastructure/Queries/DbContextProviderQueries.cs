using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Contracts.Models;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Queries;

/// <summary>
/// Implementação concreta da leitura pura (NoTracking) para prestadores de serviços.
/// </summary>
public sealed class DbContextProviderQueries(ProvidersDbContext context) : IProviderQueries
{
    private readonly ProvidersDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    private const string PostgresProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";
    private const string LikeEscapeChar = "\\";

    private bool IsPostgres => _context.Database.ProviderName == PostgresProviderName;

    private IQueryable<Provider> GetProvidersQuery()
    {
        var query = _context.Providers
            .AsNoTracking()
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Include(p => p.Services);

        return _context.Database.IsRelational() 
            ? query.AsSplitQuery() 
            : query;
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

        var idList = ids.ToList();
        return await GetProvidersQuery()
            .Where(p => idList.Contains(p.Id.Value) && !p.IsDeleted)
            .OrderBy(p => p.Id.Value)
            .ToListAsync(cancellationToken);
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
        var query = GetProvidersQuery().Where(p => !p.IsDeleted);
        
        if (IsPostgres)
        {
            var escapedCity = EscapeLikePattern(city);
            var likePattern = $"%{escapedCity}%";
            query = query.Where(p => EF.Functions.ILike(p.BusinessProfile!.PrimaryAddress!.City, likePattern, LikeEscapeChar));
            return await query.OrderBy(p => p.Id.Value).ToListAsync(cancellationToken);
        }

        var providers = await query.ToListAsync(cancellationToken);
        var lowerCity = city.ToLower();
        return providers
            .Where(p => p.BusinessProfile?.PrimaryAddress?.City?.ToLower().Contains(lowerCity) == true)
            .OrderBy(p => p.Id.Value)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Provider>> GetByStateAsync(string state, CancellationToken cancellationToken = default)
    {
        var query = GetProvidersQuery().Where(p => !p.IsDeleted);
        
        if (IsPostgres)
        {
            var escapedState = EscapeLikePattern(state);
            var likePattern = $"%{escapedState}%";
            query = query.Where(p => EF.Functions.ILike(p.BusinessProfile!.PrimaryAddress!.State, likePattern, LikeEscapeChar));
            return await query.OrderBy(p => p.Id.Value).ToListAsync(cancellationToken);
        }

        var providers = await query.ToListAsync(cancellationToken);
        var lowerState = state.ToLower();
        return providers
            .Where(p => p.BusinessProfile?.PrimaryAddress?.State?.ToLower().Contains(lowerState) == true)
            .OrderBy(p => p.Id.Value)
            .ToList();
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

        var query = GetProvidersQuery().Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            if (!_context.Database.IsRelational())
            {
                // Filtro client-side após ToListAsync para evitar erros de tradução
                var allProviders = await query.ToListAsync(cancellationToken);
                
                if (typeFilter.HasValue) allProviders = allProviders.Where(p => p.Type == typeFilter.Value).ToList();
                if (verificationStatusFilter.HasValue) allProviders = allProviders.Where(p => p.VerificationStatus == verificationStatusFilter.Value).ToList();
                
                var lowerNameFilter = nameFilter.ToLower();
                var filtered = allProviders.Where(p => p.Name.ToLower().Contains(lowerNameFilter)).ToList();
                
                var filteredCount = filtered.Count;
                var pagedItems = filtered
                    .OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToList();
                
                return new PagedResult<Provider>
                {
                    Items = pagedItems,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalItems = filteredCount
                };
            }
            
            var escapedFilter = EscapeLikePattern(nameFilter);
            var likePattern = $"%{escapedFilter}%";
            
            if (IsPostgres)
            {
                query = query.Where(p => EF.Functions.ILike(p.Name, likePattern, LikeEscapeChar));
            }
            else
            {
                query = query.Where(p => EF.Functions.Like(p.Name, likePattern, LikeEscapeChar));
            }
        }

        // Aplica filtro por tipo
        if (typeFilter.HasValue)
        {
            query = query.Where(p => p.Type == typeFilter.Value);
        }

        // Aplica filtro por status de verificação
        if (verificationStatusFilter.HasValue)
        {
            query = query.Where(p => p.VerificationStatus == verificationStatusFilter.Value);
        }

        // Ordena por data de criação (mais recentes primeiro) com ID como tiebreaker
        query = query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var providers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Provider>
        {
            Items = providers,
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
