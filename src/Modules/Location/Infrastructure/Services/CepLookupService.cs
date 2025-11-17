using MeAjudaAi.Modules.Location.Application.Services;
using MeAjudaAi.Modules.Location.Domain.ValueObjects;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Location.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de consulta de CEP com fallback chain e caching.
/// Ordem de tentativa: ViaCEP → BrasilAPI → OpenCEP
/// Cache Redis: TTL de 24 horas (CEPs são estáveis)
/// </summary>
public sealed class CepLookupService : ICepLookupService
{
    private readonly ViaCepClient _viaCepClient;
    private readonly BrasilApiCepClient _brasilApiClient;
    private readonly OpenCepClient _openCepClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CepLookupService> _logger;

    public CepLookupService(
        ViaCepClient viaCepClient,
        BrasilApiCepClient brasilApiClient,
        OpenCepClient openCepClient,
        ICacheService cacheService,
        ILogger<CepLookupService> logger)
    {
        _viaCepClient = viaCepClient;
        _brasilApiClient = brasilApiClient;
        _openCepClient = openCepClient;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Address?> LookupAsync(Cep cep, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(cep);

        // Tentar buscar do cache primeiro (TTL: 24 horas)
        var address = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                _logger.LogInformation("Cache miss para CEP {Cep}, consultando APIs", cep.Value);
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
        _logger.LogInformation("Iniciando consulta de CEP {Cep}", cep.Value);

        // Tentativa 1: ViaCEP (geralmente o mais rápido e confiável)
        var address = await _viaCepClient.GetAddressAsync(cep, cancellationToken);
        if (address is not null)
        {
            _logger.LogInformation("CEP {Cep} encontrado no ViaCEP", cep.Value);
            return address;
        }

        _logger.LogWarning("ViaCEP falhou para CEP {Cep}, tentando BrasilAPI", cep.Value);

        // Tentativa 2: BrasilAPI
        address = await _brasilApiClient.GetAddressAsync(cep, cancellationToken);
        if (address is not null)
        {
            _logger.LogInformation("CEP {Cep} encontrado no BrasilAPI", cep.Value);
            return address;
        }

        _logger.LogWarning("BrasilAPI falhou para CEP {Cep}, tentando OpenCEP", cep.Value);

        // Tentativa 3: OpenCEP (último fallback)
        address = await _openCepClient.GetAddressAsync(cep, cancellationToken);
        if (address is not null)
        {
            _logger.LogInformation("CEP {Cep} encontrado no OpenCEP", cep.Value);
            return address;
        }

        _logger.LogError("CEP {Cep} não encontrado em nenhum provedor", cep.Value);
        return null;
    }

    private static string GetCacheKey(Cep cep)
    {
        return $"cep:{cep.Value}";
    }
}
