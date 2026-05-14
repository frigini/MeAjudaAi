using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

namespace MeAjudaAi.E2E.Tests.Infrastructure.Mocks;

/// <summary>
/// Serviço de mock para geocodificação utilizado em testes E2E.
/// Provê resultados estáticos e determinísticos para evitar chamadas a APIs externas.
/// </summary>
public class MockGeocodingService : IGeocodingService
{
    /// <summary>
    /// Retorna coordenadas geográficas pré-definidas com base no endereço fornecido.
    /// </summary>
    /// <param name="address">Endereço a ser geocodificado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Retorna um <see cref="GeoPoint"/> contendo as coordenadas ou nulo se não encontrado.</returns>
    public Task<GeoPoint?> GetCoordinatesAsync(string address, CancellationToken cancellationToken = default)
    {
        // Muriaé, MG (Cidade permitida)
        if (address.Contains("Muriaé", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<GeoPoint?>(new GeoPoint(-21.139, -42.366));
        
        // Itaperuna, RJ (Cidade permitida)
        if (address.Contains("Itaperuna", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<GeoPoint?>(new GeoPoint(-21.206, -41.888));
        
        // Padrão: retorna coordenadas de Muriaé para garantir que estamos sempre em uma região permitida
        return Task.FromResult<GeoPoint?>(new GeoPoint(-21.139, -42.366));
    }

    /// <summary>
    /// Busca candidatos a localizações baseados em uma query de texto.
    /// </summary>
    /// <param name="query">Termo de busca para a localização.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Retorna uma lista estática de <see cref="LocationCandidate"/> contendo resultados para fins de teste.</returns>
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
