namespace MeAjudaAi.ApiService.Options;

/// <summary>
/// Opções de configuração para restrição geográfica.
/// Permite limitar acesso da plataforma a cidades/estados específicos (MVP piloto).
/// </summary>
/// <remarks>
/// NOTA: O controle de habilitação é feito via Microsoft.FeatureManagement (FeatureFlags.GeographicRestriction).
/// Esta classe contém apenas a configuração de quais regiões são permitidas.
/// </remarks>
public class GeographicRestrictionOptions
{
    /// <summary>
    /// Lista de estados permitidos (siglas de 2 letras, ex: "SP", "RJ").
    /// Se vazio, validação de estado é ignorada.
    /// </summary>
    public List<string> AllowedStates { get; set; } = [];

    /// <summary>
    /// Lista de cidades permitidas (nomes completos, ex: "São Paulo").
    /// Validação case-insensitive.
    /// Se vazia, a validação geográfica será ignorada.
    /// </summary>
    public List<string> AllowedCities { get; set; } = [];

    /// <summary>
    /// Mensagem exibida quando acesso é bloqueado.
    /// Placeholder {allowedRegions} será substituído pelas regiões permitidas pelo GeographicRestrictionMiddleware.
    /// </summary>
    public string BlockedMessage { get; set; } =
        "Serviço indisponível na sua região. Disponível apenas em: {allowedRegions}";
}
