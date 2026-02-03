using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de geocoding usando Nominatim (OpenStreetMap).
/// Inclui caching Redis para reduzir chamadas à API externa.
/// </summary>
public sealed class GeocodingService(
    NominatimClient nominatimClient,
    ICacheService cacheService,
    ILogger<GeocodingService> logger) : IGeocodingService
{
    public async Task<GeoPoint?> GetCoordinatesAsync(string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        var cacheKey = GetCacheKey(address);

        // Tentar buscar do cache primeiro (TTL: 7 dias)
        var coordinates = await cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                logger.LogInformation("Cache miss para geocoding de {Address}, consultando Nominatim", address);
                return await nominatimClient.GetCoordinatesAsync(address, ct);
            },
            expiration: TimeSpan.FromDays(7),
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(7),
                LocalCacheExpiration = TimeSpan.FromHours(1) // Cache local mais curto
            },
            tags: ["geocoding"],
            cancellationToken: cancellationToken);

        return coordinates;
    }



    public async Task<List<MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs.LocationCandidate>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var normalizedQuery = query.Trim().ToLowerInvariant();
        var cacheKey = $"geocoding:search:{normalizedQuery}";

        var candidates = await cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                logger.LogInformation("Cache miss para busca de locations '{Query}', consultando Nominatim", query);
                var results = await nominatimClient.SearchAsync(query, ct);
                return results.ToList();
            },
            expiration: TimeSpan.FromDays(1),
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(1),
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            tags: ["geocoding"],
            cancellationToken: cancellationToken);

        return candidates ?? [];
    }

    private static string GetCacheKey(string address)
    {
        // Normalizar endereço para evitar duplicatas de cache
        var normalized = address.Trim().ToLowerInvariant();
        return $"geocoding:{normalized}";
    }
}

