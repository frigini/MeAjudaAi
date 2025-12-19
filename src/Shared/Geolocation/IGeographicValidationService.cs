namespace MeAjudaAi.Shared.Geolocation;

/// <summary>
/// Serviço de validação geográfica para restrição de acesso por localização.
/// Utiliza um provedor externo de dados geográficos (ex.: IBGE) para normalização e validação de cidades/estados.
/// Usado pelo GeographicRestrictionMiddleware para validar cidades/estados permitidos.
/// </summary>
public interface IGeographicValidationService
{
    /// <summary>
    /// Valida se uma cidade está na lista de regiões permitidas (MVP cidades piloto).
    /// Usa um provedor externo de dados geográficos (ex.: IBGE) para normalização e validação precisa.
    /// A validação é feita contra o banco de dados (tabela AllowedCities).
    /// </summary>
    /// <param name="cityName">Nome da cidade (case-insensitive, aceita acentos). Não deve ser null ou vazio.</param>
    /// <param name="stateSigla">Sigla do estado (opcional, ex: "MG", "RJ", "ES")</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se a cidade está permitida, False caso contrário</returns>
    Task<bool> ValidateCityAsync(
        string cityName,
        string? stateSigla,
        CancellationToken cancellationToken = default);
}
