using MeAjudaAi.Modules.Location.Application.Services;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Location.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de geocoding usando Nominatim (OpenStreetMap).
/// Inclui caching Redis para reduzir chamadas à API externa.
/// </summary>
public sealed class GeocodingService : IGeocodingService
{
    private readonly NominatimClient _nominatimClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GeocodingService> _logger;

    public GeocodingService(
        NominatimClient nominatimClient,
        ICacheService cacheService,
        ILogger<GeocodingService> logger)
    {
        _nominatimClient = nominatimClient;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GeoPoint?> GetCoordinatesAsync(string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        var cacheKey = GetCacheKey(address);

        // Tentar buscar do cache primeiro (TTL: 7 dias)
        var coordinates = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                _logger.LogInformation("Cache miss para geocoding de {Address}, consultando Nominatim", address);
                return await _nominatimClient.GetCoordinatesAsync(address, ct);
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

    private static string GetCacheKey(string address)
    {
        // Normalizar endereço para evitar duplicatas de cache
        var normalized = address.Trim().ToLowerInvariant();
        return $"geocoding:{normalized}";
    }
}

