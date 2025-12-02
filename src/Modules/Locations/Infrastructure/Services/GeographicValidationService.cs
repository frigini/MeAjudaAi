using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Services;

/// <summary>
/// Adapter que implementa IGeographicValidationService delegando para IIbgeService.
/// Bridge entre Shared (middleware) e módulo Locations (IBGE).
/// </summary>
public sealed class GeographicValidationService(
    IIbgeService ibgeService,
    ILogger<GeographicValidationService> logger) : IGeographicValidationService
{
    public async Task<bool> ValidateCityAsync(
        string cityName,
        string? stateSigla,
        IReadOnlyCollection<string> allowedCities,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug(
            "GeographicValidationService: Validando cidade {CityName} (UF: {State})",
            cityName,
            stateSigla ?? "N/A");

        // Delegar para o IbgeService
        // Exceções são propagadas para o middleware decidir (fail-open com fallback)
        var isAllowed = await ibgeService.ValidateCityInAllowedRegionsAsync(
            cityName,
            stateSigla,
            allowedCities,
            cancellationToken);

        return isAllowed;
    }
}
