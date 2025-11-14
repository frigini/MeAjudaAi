using MeAjudaAi.Modules.Search.Domain.Entities;
using MeAjudaAi.Modules.Search.Domain.Enums;
using MeAjudaAi.Modules.Search.Domain.Repositories;
using MeAjudaAi.Modules.Search.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MeAjudaAi.Modules.Search.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for SearchableProvider using PostGIS for geospatial queries.
/// </summary>
public sealed class SearchableProviderRepository : ISearchableProviderRepository
{
    private readonly SearchDbContext _context;

    public SearchableProviderRepository(SearchDbContext context)
    {
        _context = context;
    }

    public async Task<SearchableProvider?> GetByIdAsync(SearchableProviderId id, CancellationToken cancellationToken = default)
    {
        return await _context.SearchableProviders
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<SearchableProvider?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await _context.SearchableProviders
            .FirstOrDefaultAsync(p => p.ProviderId == providerId, cancellationToken);
    }

    public async Task<(IReadOnlyList<SearchableProvider> Providers, int TotalCount)> SearchAsync(
        GeoPoint location,
        double radiusInKm,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        ESubscriptionTier[]? subscriptionTiers = null,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        // Create a Point for the search location (NetTopologySuite)
        var searchPoint = new Point(location.Longitude, location.Latitude) { SRID = 4326 };
        var radiusInMeters = radiusInKm * 1000;

        // Build base query - filter by active status only
        var query = _context.SearchableProviders
            .Where(p => p.IsActive);

        // Apply service filter if provided
        if (serviceIds != null && serviceIds.Length > 0)
        {
            query = query.Where(p => p.ServiceIds.Any(sid => serviceIds.Contains(sid)));
        }

        // Apply minimum rating filter
        if (minRating.HasValue)
        {
            query = query.Where(p => p.AverageRating >= minRating.Value);
        }

        // Apply subscription tier filter
        if (subscriptionTiers != null && subscriptionTiers.Length > 0)
        {
            query = query.Where(p => subscriptionTiers.Contains(p.SubscriptionTier));
        }

        // Get all matching providers (filtered by non-spatial criteria)
        // Then filter and sort by distance in-memory
        var allProviders = await query.ToListAsync(cancellationToken);

        // Apply spatial filtering and sorting in-memory
        var filteredAndSorted = allProviders
            .Select(p => new { Provider = p, Distance = location.DistanceTo(p.Location) })
            .Where(x => x.Distance <= radiusInKm)
            .OrderByDescending(x => x.Provider.SubscriptionTier)
            .ThenByDescending(x => x.Provider.AverageRating)
            .ThenBy(x => x.Distance)
            .ToList();

        var totalCount = filteredAndSorted.Count;
        var providers = filteredAndSorted
            .Skip(skip)
            .Take(take)
            .Select(x => x.Provider)
            .ToList();

        return (providers, totalCount);
    }

    public async Task AddAsync(SearchableProvider provider, CancellationToken cancellationToken = default)
    {
        await _context.SearchableProviders.AddAsync(provider, cancellationToken);
    }

    public Task UpdateAsync(SearchableProvider provider, CancellationToken cancellationToken = default)
    {
        _context.SearchableProviders.Update(provider);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SearchableProvider provider, CancellationToken cancellationToken = default)
    {
        _context.SearchableProviders.Remove(provider);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
