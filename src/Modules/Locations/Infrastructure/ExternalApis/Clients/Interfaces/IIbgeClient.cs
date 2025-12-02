using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients.Interfaces;

/// <summary>
/// Interface para cliente HTTP da API IBGE Localidades.
/// Permite mocking em unit tests.
/// </summary>
public interface IIbgeClient
{
    Task<Municipio?> GetMunicipioByNameAsync(string cityName, CancellationToken cancellationToken = default);
    Task<List<Municipio>> GetMunicipiosByUFAsync(string ufSigla, CancellationToken cancellationToken = default);
    Task<bool> ValidateCityInStateAsync(string city, string state, CancellationToken cancellationToken = default);
}
