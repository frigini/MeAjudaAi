using MeAjudaAi.Modules.Search.Domain.Entities;
using MeAjudaAi.Modules.Search.Domain.Enums;
using MeAjudaAi.Modules.Search.Domain.Models;
using MeAjudaAi.Modules.Search.Domain.Repositories;
using MeAjudaAi.Modules.Search.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MeAjudaAi.Modules.Search.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação de repositório para SearchableProvider.
/// 
/// NOTA SOBRE PERFORMANCE:
/// Atualmente realiza filtragem espacial (distância/raio) em memória após carregar
/// provedores filtrados do banco. Filtros não-espaciais (serviços, avaliação, tier)
/// são aplicados no nível do banco de dados.
/// 
/// LIMITAÇÃO DO EF CORE:
/// A propriedade Location usa HasConversion (GeoPoint <-> NTS.Point), o que impede
/// o EF Core de traduzir funções espaciais NTS (IsWithinDistance, Distance) para SQL.
/// Soluções futuras: usar FromSqlInterpolated ou remover conversão customizada.
/// 
/// ESCALABILIDADE:
/// Para datasets pequenos/médios (milhares de provedores), a abordagem híbrida funciona bem.
/// Para datasets grandes (milhões), considerar:
/// 1. Remover HasConversion e usar GeoJSON/WKT
/// 2. Usar FromSqlRaw com ST_DWithin/ST_Distance do PostGIS
/// 3. Implementar caching de resultados geoespaciais
/// </summary>
public sealed class SearchableProviderRepository(SearchDbContext context) : ISearchableProviderRepository
{
    public async Task<SearchableProvider?> GetByIdAsync(SearchableProviderId id, CancellationToken cancellationToken = default)
    {
        return await context.SearchableProviders
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<SearchableProvider?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await context.SearchableProviders
            .FirstOrDefaultAsync(p => p.ProviderId == providerId, cancellationToken);
    }

    public async Task<SearchResult> SearchAsync(
        GeoPoint location,
        double radiusInKm,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        ESubscriptionTier[]? subscriptionTiers = null,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        // Build base query - filter by active status (uses ix_searchable_providers_is_active index)
        var query = context.SearchableProviders
            .AsNoTracking()
            .Where(p => p.IsActive);

        // Apply service filter using PostgreSQL array overlap operator
        // EF Core translates ServiceIds.Any() to PostgreSQL && operator
        // This uses the GIN index on service_ids column for efficient filtering
        if (serviceIds != null && serviceIds.Length > 0)
        {
            query = query.Where(p => p.ServiceIds.Any(sid => serviceIds.Contains(sid)));
        }

        // Apply minimum rating filter (uses ix_searchable_providers_search_ranking composite index)
        if (minRating.HasValue)
        {
            query = query.Where(p => p.AverageRating >= minRating.Value);
        }

        // Apply subscription tier filter (uses ix_searchable_providers_subscription_tier index)
        if (subscriptionTiers != null && subscriptionTiers.Length > 0)
        {
            query = query.Where(p => subscriptionTiers.Contains(p.SubscriptionTier));
        }

        // Execute non-spatial filters in database (optimized with indexes)
        var allProviders = await query.ToListAsync(cancellationToken);

        // Apply spatial filtering in-memory with cached distances
        // REASON: EF Core cannot translate Location property (has HasConversion) to PostGIS functions
        // The GeoPoint -> Point conversion prevents query translation
        // Cache distance per provider to avoid redundant calculations during filter/sort/map
        var withDistance = allProviders
            .Select(p => new { Provider = p, Distance = p.CalculateDistanceToInKm(location) })
            .Where(x => x.Distance <= radiusInKm)
            .ToList();

        var totalCount = withDistance.Count;

        // Apply sorting and pagination in-memory
        // Order: SubscriptionTier (desc) -> AverageRating (desc) -> Distance (asc)
        // Guard pagination parameters to prevent negative OFFSET/LIMIT reaching PostgreSQL
        var paginated = withDistance
            .OrderByDescending(x => x.Provider.SubscriptionTier)
            .ThenByDescending(x => x.Provider.AverageRating)
            .ThenBy(x => x.Distance)
            .Skip(Math.Max(0, skip))
            .Take(Math.Max(0, take))
            .ToList();

        return new SearchResult
        {
            Providers = paginated.Select(x => x.Provider).ToList(),
            DistancesInKm = paginated.Select(x => x.Distance).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task AddAsync(SearchableProvider provider, CancellationToken cancellationToken = default)
    {
        await context.SearchableProviders.AddAsync(provider, cancellationToken);
    }

    public Task UpdateAsync(SearchableProvider provider, CancellationToken cancellationToken = default)
    {
        context.SearchableProviders.Update(provider);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SearchableProvider provider, CancellationToken cancellationToken = default)
    {
        context.SearchableProviders.Remove(provider);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
