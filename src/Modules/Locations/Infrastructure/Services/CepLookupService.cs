using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Enums;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de consulta de CEP com fallback chain e caching.
/// Ordem de tentativa: ViaCEP → BrasilAPI → OpenCEP
/// Cache Redis: TTL de 24 horas (CEPs são estáveis)
/// </summary>
public sealed class CepLookupService(
    ViaCepClient viaCepClient,
    BrasilApiCepClient brasilApiClient,
    OpenCepClient openCepClient,
    ICacheService cacheService,
    ILogger<CepLookupService> logger) : ICepLookupService
{
    // Ordem padrão de fallback (pode ser configurada no futuro)
    private static readonly ECepProvider[] DefaultProviderOrder =
    {
        ECepProvider.ViaCep,
        ECepProvider.BrasilApi,
        ECepProvider.OpenCep
    };

    public async Task<Address?> LookupAsync(Cep cep, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(cep);

        // Tentar buscar do cache primeiro (TTL: 24 horas)
        var address = await cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                logger.LogInformation("Cache miss para CEP {Cep}, consultando APIs", cep.Value);
                return await LookupFromProvidersAsync(cep, ct);
            },
            expiration: TimeSpan.FromHours(24),
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(24),
                LocalCacheExpiration = TimeSpan.FromMinutes(30) // Cache local mais curto
            },
            tags: ["cep", $"cep:{cep.Value}"],
            cancellationToken: cancellationToken);

        return address;
    }

    private async Task<Address?> LookupFromProvidersAsync(Cep cep, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting CEP lookup {Cep}", cep.Value);

        foreach (var provider in DefaultProviderOrder)
        {
            var address = await TryProviderAsync(provider, cep, cancellationToken);
            if (address is not null)
            {
                logger.LogInformation("CEP {Cep} found in provider {Provider}", cep.Value, provider);
                return address;
            }

            logger.LogWarning("Provider {Provider} failed for CEP {Cep}, trying next", provider, cep.Value);
        }

        logger.LogError("CEP {Cep} not found in any provider", cep.Value);
        return null;
    }

    private async Task<Address?> TryProviderAsync(ECepProvider provider, Cep cep, CancellationToken cancellationToken)
    {
        return provider switch
        {
            ECepProvider.ViaCep => await viaCepClient.GetAddressAsync(cep, cancellationToken),
            ECepProvider.BrasilApi => await brasilApiClient.GetAddressAsync(cep, cancellationToken),
            ECepProvider.OpenCep => await openCepClient.GetAddressAsync(cep, cancellationToken),
            _ => null
        };
    }

    private static string GetCacheKey(Cep cep)
    {
        return $"cep:{cep.Value}";
    }
}
