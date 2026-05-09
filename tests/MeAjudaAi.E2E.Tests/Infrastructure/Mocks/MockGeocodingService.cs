using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

namespace MeAjudaAi.E2E.Tests.Infrastructure.Mocks;

public class MockGeocodingService : IGeocodingService
{
    public Task<GeoPoint?> GetCoordinatesAsync(string address, CancellationToken cancellationToken = default)
    {
        // Muriaé, MG (Allowed city)
        if (address.Contains("Muriaé", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<GeoPoint?>(new GeoPoint(-21.139, -42.366));
        
        // Itaperuna, RJ (Allowed city)
        if (address.Contains("Itaperuna", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<GeoPoint?>(new GeoPoint(-21.206, -41.888));
        
        // Default: return Muriaé coordinates to ensure we are always in an allowed region
        return Task.FromResult<GeoPoint?>(new GeoPoint(-21.139, -42.366));
    }

    public Task<List<LocationCandidate>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<LocationCandidate>
        {
            new LocationCandidate(
                "Muriaé, MG, Brasil",
                "Muriaé",
                "MG",
                "Brasil",
                -21.139,
                -42.366
            )
        });
    }
}
