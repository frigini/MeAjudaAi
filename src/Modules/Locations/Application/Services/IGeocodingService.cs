using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Modules.Locations.Application.Services;

/// <summary>
/// Serviço de geocoding para converter endereços em coordenadas.
/// </summary>
public interface IGeocodingService
{
    /// <summary>
    /// Obtém coordenadas geográficas a partir de um endereço completo.
    /// </summary>
    Task<GeoPoint?> GetCoordinatesAsync(string address, CancellationToken cancellationToken = default);
}
