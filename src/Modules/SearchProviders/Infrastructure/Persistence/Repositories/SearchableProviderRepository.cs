using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.Models;
using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.DTOs;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação de repositório para SearchableProvider.
/// 
/// ARQUITETURA HÍBRIDA (EF Core + Dapper):
/// Este repositório usa o melhor de cada ORM para diferentes tipos de operações:
/// 
/// EF CORE (Operações CRUD):
/// - GetByIdAsync, GetByProviderIdAsync → type-safe, navegação simples
/// - AddAsync, UpdateAsync, DeleteAsync → change tracking, validações automáticas
/// - SaveChangesAsync → transações gerenciadas, unit of work pattern
/// - Mantém encapsulamento do domínio (GeoPoint value object, validações)
/// 
/// DAPPER + POSTGIS NATIVO (Queries Espaciais):
/// - SearchAsync → raw SQL com ST_DWithin e ST_Distance do PostGIS
/// - Aproveita índices GIST espaciais para máxima performance
/// - Filtragem/ordenação/paginação executadas no banco de dados
/// - Resolve limitação do EF Core que não traduz HasConversion para funções espaciais
/// 
/// POR QUE HÍBRIDO?
/// - EF Core não consegue traduzir Location (HasConversion GeoPoint to NTS.Point) para SQL espacial
/// - Remover HasConversion quebraria encapsulamento do domínio
/// - Dapper para tudo seria overhead desnecessário (sem change tracking, mapeamento manual)
/// - Solução: use cada ferramenta onde ela brilha
/// 
/// PERFORMANCE:
/// - Queries espaciais executam ST_DWithin/ST_Distance diretamente no PostGIS
/// - Índices GIST são utilizados (ix_searchable_providers_location)
/// - Paginação acontece no banco (OFFSET/LIMIT), não em memória
/// - Distâncias calculadas uma única vez no SQL, retornadas com os resultados
/// 
/// MAPEAMENTO:
/// - ProviderSearchResultDto (interno) to SearchableProvider (domínio)
/// - Usa IDapperConnection do Shared (já configurado com métricas e logging)
/// - Mantém todas as invariantes e validações do domínio
/// </summary>
public sealed class SearchableProviderRepository(
    SearchProvidersDbContext context,
    IDapperConnection dapper) : ISearchableProviderRepository
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
        // ST_DWithin filtra por raio usando índice GIST
        // ST_Distance calcula distância exata em metros
        var whereClause = BuildWhereClauses(searchPattern, serviceIds, minRating, subscriptionTiers);
        
        var sql = $"""
            SELECT 
                id AS Id,
                provider_id AS ProviderId,
                name AS Name,
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
            ORDER BY SubscriptionTier DESC, AverageRating DESC, DistanceKm ASC
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

    /// <summary>
    /// Sanitiza entrada do usuário para uso seguro com cláusulas ILIKE/LIKE.
    /// Retorna string nula se a entrada for vazia.
    /// Escapa caracteres especiais de wildcard (%, _, \) com backslash.
    /// </summary>
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
        // Usar método Reconstitute do domínio para reconstruir entidade existente do banco
        // (em vez de Create que geraria novo ID)
        return SearchableProvider.Reconstitute(
            id: dto.Id,
            providerId: dto.ProviderId,
            name: dto.Name,
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
