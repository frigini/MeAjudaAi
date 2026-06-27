using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Application.ModuleApi;

/// <summary>
/// Implementação da API pública do módulo Locations para outros módulos.
/// </summary>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class LocationsModuleApi(
    ICepLookupService cepLookupService,
    IGeocodingService geocodingService,
    IAllowedCityQueries allowedCityQueries,
    ILogger<LocationsModuleApi> logger) : ILocationsModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = ModuleNames.Locations;
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    // CEP real usado para health check - validação end-to-end de conectividade com APIs externas
    private const string HealthCheckCep = "01310100";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking Location module availability");

            // Testa operação básica - busca por um CEP conhecido
            var testCep = Cep.Create(HealthCheckCep); // Av. Paulista, São Paulo
            if (testCep is not null)
            {
                _ = await cepLookupService.LookupAsync(testCep, cancellationToken);
                // Se conseguiu fazer a requisição (mesmo que retorne null), o módulo está disponível
                logger.LogDebug("Location module is available and healthy");
                return true;
            }

            logger.LogWarning("Location module unavailable - basic operations test failed");
            return false;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error while checking Location module availability");
            return false;
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "Timeout while checking Location module availability");
            return false;
        }
    }

    public async Task<Result<ModuleAddressDto>> GetAddressFromCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        var cepValueObject = Cep.Create(cep);
        if (cepValueObject is null)
        {
            return Result<ModuleAddressDto>.Failure($"CEP inválido: {cep}");
        }

        var address = await cepLookupService.LookupAsync(cepValueObject, cancellationToken);
        if (address is null)
        {
            return Result<ModuleAddressDto>.Failure($"CEP {cep} não encontrado");
        }

        var dto = new ModuleAddressDto(
            address.Cep.Formatted,
            address.Street,
            address.Neighborhood,
            address.City,
            address.State,
            address.Complement,
            address.GeoPoint is not null
                ? new ModuleCoordinatesDto(address.GeoPoint.Latitude, address.GeoPoint.Longitude)
                : null);

        return Result<ModuleAddressDto>.Success(dto);
    }

    public async Task<Result<ModuleCoordinatesDto>> GetCoordinatesFromAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return Result<ModuleCoordinatesDto>.Failure("Endereço não pode ser vazio");
        }

        var coordinates = await geocodingService.GetCoordinatesAsync(address, cancellationToken);
        if (coordinates is null)
        {
            return Result<ModuleCoordinatesDto>.Failure($"Coordenadas não encontradas para o endereço: {address}");
        }

        var dto = new ModuleCoordinatesDto(coordinates.Latitude, coordinates.Longitude);
        return Result<ModuleCoordinatesDto>.Success(dto);
    }

    public async Task<Result<Guid?>> GetAllowedCityIdAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default)
    {
        try
        {
            var city = await allowedCityQueries.GetByCityAndStateAsync(cityName, stateSigla, cancellationToken);
            return Result<Guid?>.Success(city?.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting allowed city ID for {CityName}/{StateSigla}", cityName, stateSigla);
            return Result<Guid?>.Failure($"Erro ao buscar ID da cidade: {ex.Message}");
        }
    }
}
