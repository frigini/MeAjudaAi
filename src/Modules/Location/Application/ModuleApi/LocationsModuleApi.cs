using MeAjudaAi.Modules.Location.Application.Services;
using MeAjudaAi.Modules.Location.Domain.ValueObjects;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.Location;
using MeAjudaAi.Shared.Contracts.Modules.Location.DTOs;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Location.Application.ModuleApi;

/// <summary>
/// Implementação da API pública do módulo Location para outros módulos.
/// </summary>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class LocationsModuleApi(
    ICepLookupService cepLookupService,
    IGeocodingService geocodingService,
    ILogger<LocationsModuleApi> logger) : ILocationModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = "Location";
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
                var result = await cepLookupService.LookupAsync(testCep, cancellationToken);
                // Se conseguiu fazer a requisição (mesmo que retorne null), o módulo está disponível
                logger.LogDebug("Location module is available and healthy");
                return true;
            }

            logger.LogWarning("Location module unavailable - basic operations test failed");
            return false;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Location module availability check was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Location module availability");
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
}
