using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

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
    
    /// <summary>
    /// Busca endereços/cidades que correspondam à query.
    /// </summary>
    Task<List<LocationCandidate>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
