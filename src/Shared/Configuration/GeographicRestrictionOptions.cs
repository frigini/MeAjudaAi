namespace MeAjudaAi.Shared.Configuration;

/// <summary>
/// Opções de configuração para restrição geográfica.
/// Permite limitar acesso da plataforma a cidades/estados específicos (MVP piloto).
/// </summary>
public class GeographicRestrictionOptions
{
    /// <summary>
    /// Habilita/desabilita a restrição geográfica.
    /// Development: false (permitir tudo)
    /// Production: true (apenas cidades piloto)
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Lista de estados permitidos (siglas de 2 letras, ex: "SP", "RJ").
    /// Se vazio, validação de estado é ignorada.
    /// </summary>
    public List<string> AllowedStates { get; set; } = [];

    /// <summary>
    /// Lista de cidades permitidas (nomes completos, ex: "São Paulo").
    /// Validação case-insensitive.
    /// </summary>
    public List<string> AllowedCities { get; set; } = [];

    /// <summary>
    /// Mensagem exibida quando acesso é bloqueado.
    /// Placeholder {allowedRegions} será substituído pelas regiões permitidas.
    /// </summary>
    public string BlockedMessage { get; set; } =
        "Serviço indisponível na sua região. Disponível apenas em: {allowedRegions}";
}
