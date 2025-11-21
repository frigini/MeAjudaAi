namespace MeAjudaAi.Shared.Geolocation;

/// <summary>
/// Serviço de validação geográfica para restrição de acesso por localização.
/// Usado pelo GeographicRestrictionMiddleware para validar cidades/estados permitidos.
/// </summary>
public interface IGeographicValidationService
{
    /// <summary>
    /// Valida se uma cidade está na lista de regiões permitidas (MVP cidades piloto).
    /// Usa API IBGE para normalização e validação precisa.
    /// </summary>
    /// <param name="cityName">Nome da cidade (case-insensitive, aceita acentos)</param>
    /// <param name="stateSigla">Sigla do estado (opcional, ex: "MG", "RJ", "ES")</param>
    /// <param name="allowedCities">Lista de cidades permitidas do appsettings</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se a cidade está permitida, False caso contrário</returns>
    Task<bool> ValidateCityAsync(
        string cityName, 
        string? stateSigla, 
        IEnumerable<string> allowedCities,
        CancellationToken cancellationToken = default);
}
