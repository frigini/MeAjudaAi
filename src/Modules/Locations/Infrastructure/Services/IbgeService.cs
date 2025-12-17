using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients.Interfaces;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de validação geográfica usando API IBGE com caching.
/// Cache Redis: TTL de 7 dias (municípios raramente mudam)
/// </summary>
public sealed class IbgeService(
    IIbgeClient ibgeClient,
    ICacheService cacheService,
    IAllowedCityRepository allowedCityRepository,
    ILogger<IbgeService> logger) : IIbgeService
{
    public async Task<bool> ValidateCityInAllowedRegionsAsync(
        string cityName,
        string? stateSigla,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Validando cidade {CityName} (UF: {State}) contra lista de cidades permitidas no banco de dados", cityName, stateSigla ?? "N/A");

        // Buscar detalhes do município na API IBGE (com cache)
        // Exceções são propagadas para GeographicValidationService -> Middleware (fail-open com fallback)
        var municipio = await GetCityDetailsAsync(cityName, cancellationToken);

        if (municipio is null)
        {
            logger.LogWarning("Municipality {CityName} not found in IBGE API — throwing exception for fallback", cityName);
            throw new MunicipioNotFoundException(cityName, stateSigla);
        }

        // Validar se o estado bate (se fornecido)
        var ufSigla = municipio.GetEstadoSigla();
        if (!string.IsNullOrEmpty(stateSigla) && !string.Equals(ufSigla, stateSigla, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "Município {CityName} encontrado, mas estado não corresponde. Esperado: {ExpectedState}, Encontrado: {FoundState}",
                cityName, stateSigla, ufSigla);
            return false;
        }

        // Validar se a cidade está na lista de permitidas (usando banco de dados)
        // ufSigla never null because GetEstadoSigla returns non-nullable string
        var isAllowed = await allowedCityRepository.IsCityAllowedAsync(municipio.Nome, ufSigla ?? string.Empty, cancellationToken);

        if (isAllowed)
        {
            logger.LogInformation("Municipality {CityName} ({Id}) is in the allowed cities list", municipio.Nome, municipio.Id);
        }
        else
        {
            logger.LogWarning("Municipality {CityName} ({Id}) is NOT in the allowed cities list", municipio.Nome, municipio.Id);
        }

        return isAllowed;
    }

    public async Task<Municipio?> GetCityDetailsAsync(string cityName, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(cityName);

        // Buscar do cache ou API IBGE (TTL: 7 dias)
        var municipio = await cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                logger.LogInformation("Cache miss para município {CityName}, consultando API IBGE", cityName);
                return await ibgeClient.GetMunicipioByNameAsync(cityName, ct);
            },
            expiration: TimeSpan.FromDays(7),
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(7),
                LocalCacheExpiration = TimeSpan.FromHours(24) // Cache local mais curto
            },
            tags: ["ibge", $"municipio:{cityName}"],
            cancellationToken: cancellationToken);

        return municipio;
    }

    public async Task<List<Municipio>> GetMunicipiosByUFAsync(string ufSigla, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKeyForUF(ufSigla);

        // Buscar do cache ou API IBGE (TTL: 7 dias)
        var municipios = await cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                logger.LogInformation("Cache miss para municípios da UF {UF}, consultando API IBGE", ufSigla);
                return await ibgeClient.GetMunicipiosByUFAsync(ufSigla, ct);
            },
            expiration: TimeSpan.FromDays(7),
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(7),
                LocalCacheExpiration = TimeSpan.FromHours(24)
            },
            tags: ["ibge", $"uf:{ufSigla}"],
            cancellationToken: cancellationToken);

        return municipios ?? [];
    }

    private static string GetCacheKey(string cityName)
    {
        return $"ibge:municipio:{cityName.ToLowerInvariant()}";
    }

    private static string GetCacheKeyForUF(string ufSigla)
    {
        return $"ibge:uf:{ufSigla.ToUpperInvariant()}";
    }
}
