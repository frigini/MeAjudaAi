using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Services;

/// <summary>
/// Implementação de geocodificação desabilitada (No-Op) para uso em ambientes onde
/// a integração externa não é necessária ou desejada.
/// </summary>
public sealed class NoOpGeocodingService : IGeocodingService
{
    public Task<GeoPoint?> GetCoordinatesAsync(string address, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<GeoPoint?>(null);
    }

    public Task<List<LocationCandidate>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<LocationCandidate>());
    }
}
