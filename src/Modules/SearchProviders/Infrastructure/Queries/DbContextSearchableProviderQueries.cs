using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.Models;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.DTOs;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Queries;

/// <summary>
/// Implementação de ISearchableProviderQueries na camada de infraestrutura utilizando EF Core e Dapper.
/// </summary>
public sealed class DbContextSearchableProviderQueries(
    SearchProvidersDbContext context,
    IDapperConnection dapper) : ISearchableProviderQueries
{
    public async Task<SearchableProvider?> GetByIdAsync(SearchableProviderId id, CancellationToken cancellationToken = default)
    {
        return await context.SearchableProviders
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<SearchableProvider?> GetByProviderIdAsync(Guid providerId, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = context.SearchableProviders.AsQueryable();
        if (!track)
        {
            query = query.AsNoTracking();
        }
        return await query.FirstOrDefaultAsync(p => p.ProviderId == providerId, cancellationToken);
    }

    public async Task<SearchResult> SearchAsync(
        GeoPoint location,
        double radiusInKm,
        string? term = null,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        ESubscriptionTier[]? subscriptionTiers = null,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        // Validar raio antes de executar query no banco
        if (radiusInKm <= 0)
        {
            return new SearchResult(
                Providers: [],
                DistancesInKm: [],
                TotalCount: 0);
        }

        // Sanitizar termo de busca para ILIKE
        string? searchPattern = ToILikePattern(term);

        // Usar Dapper com PostGIS nativo para máxima performance espacial
        var whereClause = BuildWhereClauses(searchPattern, serviceIds, minRating, subscriptionTiers);
        
        var sql = $"""
            SELECT 
                id AS Id,
                provider_id AS ProviderId,
                name AS Name,
                slug AS Slug,
                description AS Description,
                ST_Y(location::geometry) AS Latitude,
                ST_X(location::geometry) AS Longitude,
                average_rating AS AverageRating,
                total_reviews AS TotalReviews,
                subscription_tier AS SubscriptionTier,
                service_ids AS ServiceIds,
                city AS City,
                state AS State,
                is_active AS IsActive,
                ST_Distance(
                    location::geography,
                    ST_SetSRID(ST_MakePoint(@Lng, @Lat), 4326)::geography
                ) / 1000.0 AS DistanceKm
            FROM search_providers.searchable_providers
            {whereClause}
            ORDER BY subscription_tier DESC, average_rating DESC, DistanceKm ASC
            OFFSET @Skip LIMIT @Take
            """;

        var results = await dapper.QueryAsync<ProviderSearchResultDto>(
            sql,
            new
            {
                Lat = location.Latitude,
                Lng = location.Longitude,
                RadiusMeters = radiusInKm * 1000, // Converter km para metros
                Term = searchPattern,
                ServiceIds = serviceIds,
                MinRating = minRating,
                Tiers = subscriptionTiers?.Select(t => (int)t).ToArray(),
                Skip = Math.Max(0, skip),
                Take = Math.Max(0, take)
            },
            cancellationToken);

        var resultList = results.ToList();

        // Contar total antes da paginação (executar query de count separada)
        var countSql = $"""
            SELECT COUNT(*)
            FROM search_providers.searchable_providers
            {whereClause}
            """;

        var totalCount = await dapper.QuerySingleOrDefaultAsync<int?>(
            countSql,
            new
            {
                Lat = location.Latitude,
                Lng = location.Longitude,
                RadiusMeters = radiusInKm * 1000,
                Term = searchPattern,
                ServiceIds = serviceIds,
                MinRating = minRating,
                Tiers = subscriptionTiers?.Select(t => (int)t).ToArray()
            },
            cancellationToken) ?? 0;

        // Mapear DTOs de volta para entidades do domínio
        var providers = resultList.Select(MapToEntity).ToList();
        var distances = resultList.Select(r => r.DistanceKm).ToList();

        return new SearchResult(
            Providers: providers,
            DistancesInKm: distances,
            TotalCount: totalCount);
    }

    private static string BuildWhereClauses(
        string? termPattern,
        Guid[]? serviceIds,
        decimal? minRating,
        ESubscriptionTier[]? subscriptionTiers)
    {
        var termFilter = !string.IsNullOrWhiteSpace(termPattern)
            ? "AND (name ILIKE @Term ESCAPE '\\' OR description ILIKE @Term ESCAPE '\\')"
            : "";

        var serviceFilter = serviceIds?.Length > 0
            ? "AND service_ids && @ServiceIds"
            : "";

        var ratingFilter = minRating.HasValue
            ? "AND average_rating >= @MinRating"
            : "";

        var tierFilter = subscriptionTiers?.Length > 0
            ? "AND subscription_tier = ANY(@Tiers)"
            : "";

        return $"""
            WHERE is_active = true
                AND ST_DWithin(
                    location::geography,
                    ST_SetSRID(ST_MakePoint(@Lng, @Lat), 4326)::geography,
                    @RadiusMeters
                )
                {termFilter}
                {serviceFilter}
                {ratingFilter}
                {tierFilter}
            """;
    }

    private static string? ToILikePattern(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return null;

        var escapedTerm = term
            .Replace("\\", "\\\\") // Escapar o próprio caractere de escape primeiro
            .Replace("%", "\\%")   // Escapar wildcard %
            .Replace("_", "\\_");  // Escapar wildcard _

        return $"%{escapedTerm}%";
    }

    private static SearchableProvider MapToEntity(ProviderSearchResultDto dto)
    {
        // Reconstituir entidade existente do banco de dados (Dapper query)
        return SearchableProvider.Reconstitute(
            id: dto.Id,
            providerId: dto.ProviderId,
            name: dto.Name,
            slug: dto.Slug,
            location: new GeoPoint(dto.Latitude, dto.Longitude),
            subscriptionTier: (ESubscriptionTier)dto.SubscriptionTier,
            averageRating: dto.AverageRating,
            totalReviews: dto.TotalReviews,
            serviceIds: dto.ServiceIds,
            isActive: dto.IsActive,
            description: dto.Description,
            city: dto.City,
            state: dto.State);
    }

    public async Task<IReadOnlyList<SearchableProvider>> GetByServiceIdAsync(Guid serviceId, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = context.SearchableProviders.AsQueryable();
        if (!track)
        {
            query = query.AsNoTracking();
        }
        return await query
            .Where(p => p.ServiceIds.Contains(serviceId))
            .ToListAsync(cancellationToken);
    }
}


