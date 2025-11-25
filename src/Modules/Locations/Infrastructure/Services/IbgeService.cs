using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
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
    ILogger<IbgeService> logger) : IIbgeService
{
    public async Task<bool> ValidateCityInAllowedRegionsAsync(
        string cityName,
        string? stateSigla,
        IReadOnlyCollection<string> allowedCities,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Validando cidade {CityName} (UF: {State}) contra lista de cidades permitidas", cityName, stateSigla ?? "N/A");

            // Buscar detalhes do município na API IBGE (com cache)
            var municipio = await GetCityDetailsAsync(cityName, cancellationToken);

            if (municipio is null)
            {
                logger.LogWarning("Município {CityName} não encontrado na API IBGE", cityName);
                return false;
            }

            // Validar se o estado bate (se fornecido)
            if (!string.IsNullOrEmpty(stateSigla))
            {
                var ufSigla = municipio.GetEstadoSigla();
                if (!string.Equals(ufSigla, stateSigla, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning(
                        "Município {CityName} encontrado, mas estado não corresponde. Esperado: {ExpectedState}, Encontrado: {FoundState}",
                        cityName, stateSigla, ufSigla);
                    return false;
                }
            }

            // Validar se a cidade está na lista de permitidas (case-insensitive)
            var isAllowed = allowedCities.Any(allowedCity =>
                string.Equals(allowedCity, municipio.Nome, StringComparison.OrdinalIgnoreCase));

            if (isAllowed)
            {
                logger.LogInformation("Município {CityName} ({Id}) está na lista de cidades permitidas", municipio.Nome, municipio.Id);
            }
            else
            {
                logger.LogWarning("Município {CityName} ({Id}) NÃO está na lista de cidades permitidas", municipio.Nome, municipio.Id);
            }

            return isAllowed;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao validar cidade {CityName} contra API IBGE", cityName);
            return false; // Fail-closed: em caso de erro, bloquear acesso por segurança
        }
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
