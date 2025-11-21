using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;

namespace MeAjudaAi.Modules.Locations.Application.Services;

/// <summary>
/// Serviço de validação geográfica usando API IBGE.
/// </summary>
public interface IIbgeService
{
    /// <summary>
    /// Valida se uma cidade está nas regiões permitidas (cidades piloto do MVP).
    /// </summary>
    /// <param name="cityName">Nome da cidade (case-insensitive, aceita acentos)</param>
    /// <param name="stateSigla">Sigla do estado (opcional, ex: "MG", "RJ", "ES")</param>
    /// <param name="allowedCities">Lista de cidades permitidas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se a cidade está permitida, False caso contrário</returns>
    Task<bool> ValidateCityInAllowedRegionsAsync(
        string cityName,
        string? stateSigla,
        IEnumerable<string> allowedCities,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém detalhes completos de um município (incluindo hierarquia geográfica).
    /// </summary>
    Task<Municipio?> GetCityDetailsAsync(string cityName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os municípios de uma UF.
    /// </summary>
    Task<List<Municipio>> GetMunicipiosByUFAsync(string ufSigla, CancellationToken cancellationToken = default);
}
