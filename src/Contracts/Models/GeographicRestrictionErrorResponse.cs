using System.Text.Json.Serialization;

namespace MeAjudaAi.Contracts.Models;

/// <summary>
/// Modelo para erros de restrição geográfica (HTTP 451 - Unavailable For Legal Reasons).
/// </summary>
/// <remarks>
/// Utilizado quando o acesso ao serviço é bloqueado devido a restrições geográficas.
/// HTTP 451 é definido na RFC 7725 para conteúdo indisponível por razões legais ou regulatórias.
/// 
/// **Cenários de uso:**
/// - Bloqueio de acesso a APIs baseado em localização do usuário
/// - Restrições regulatórias de operação por região
/// - Compliance com regulamentações locais
/// - Piloto de serviços em cidades específicas
/// 
/// **Exemplo de resposta:**
/// ```json
/// {
///   "statusCode": 451,
///   "title": "Unavailable For Legal Reasons",
///   "detail": "Serviço indisponível na sua região. Atualmente operamos apenas em: Muriaé-MG, Itaperuna-RJ, Linhares-ES.",
///   "error": "geographic_restriction",
///   "yourLocation": {
///     "city": "São Paulo",
///     "state": "SP"
///   },
///   "allowedCities": [
///     { "name": "Muriaé", "state": "MG", "ibgeCode": "3143906" },
///     { "name": "Itaperuna", "state": "RJ", "ibgeCode": "3302205" },
///     { "name": "Linhares", "state": "ES", "ibgeCode": "3203205" }
///   ],
///   "allowedStates": ["MG", "RJ", "ES"]
/// }
/// ```
/// </remarks>
public class GeographicRestrictionErrorResponse : ApiErrorResponse
{
    /// <summary>
    /// Localização atual do usuário detectada pelos headers HTTP.
    /// </summary>
    /// <example>
    /// { "city": "São Paulo", "state": "SP" }
    /// </example>
    [JsonPropertyName("yourLocation")]
    public UserLocation? YourLocation { get; set; }

    /// <summary>
    /// Lista de cidades permitidas onde o serviço está disponível.
    /// </summary>
    /// <example>
    /// [
    ///   { "name": "Muriaé", "state": "MG", "ibgeCode": "3143906" },
    ///   { "name": "Itaperuna", "state": "RJ", "ibgeCode": "3302205" }
    /// ]
    /// </example>
    [JsonPropertyName("allowedCities")]
    public IEnumerable<AllowedCity>? AllowedCities { get; set; }

    /// <summary>
    /// Lista de estados (UFs) permitidos onde o serviço está disponível.
    /// </summary>
    /// <example>["MG", "RJ", "ES"]</example>
    [JsonPropertyName("allowedStates")]
    public IEnumerable<string>? AllowedStates { get; set; }

    /// <summary>
    /// Código de erro específico para restrição geográfica.
    /// </summary>
    /// <example>geographic_restriction</example>
    [JsonPropertyName("error")]
    public string Error { get; set; } = "geographic_restriction";

    /// <summary>
    /// Inicializa uma nova instância para erro de restrição geográfica.
    /// </summary>
    /// <param name="message">Mensagem descritiva sobre a restrição (opcional)</param>
    /// <param name="userLocation">Localização detectada do usuário (opcional)</param>
    /// <param name="allowedCities">Lista de cidades permitidas (opcional)</param>
    /// <param name="allowedStates">Lista de estados permitidos (opcional)</param>
    public GeographicRestrictionErrorResponse(
        string? message = null,
        UserLocation? userLocation = null,
        IEnumerable<AllowedCity>? allowedCities = null,
        IEnumerable<string>? allowedStates = null)
    {
        StatusCode = 451; // HTTP 451 - Unavailable For Legal Reasons (RFC 7725)
        Title = "Unavailable For Legal Reasons";
        Detail = message ?? "Serviço indisponível na sua região.";
        YourLocation = userLocation;
        AllowedCities = allowedCities;
        AllowedStates = allowedStates;
    }
}
