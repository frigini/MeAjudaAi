namespace MeAjudaAi.Gateway.Options;

/// <summary>
/// Opções de configuração para restrição geográfica.
/// </summary>
public sealed class GeographicRestrictionOptions
{
    public const string SectionName = "GeographicRestriction";

    /// <summary>
    /// Indica se a restrição geográfica está habilitada.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Lista de estados permitidos (UF).
    /// Exemplo: ["MG", "RJ", "ES"]
    /// </summary>
    public List<string>? AllowedStates { get; set; }

    /// <summary>
    /// Lista de cidades permitidas no formato "Cidade|Estado" ou apenas "Cidade".
    /// Exemplo: ["Muriaé|MG", "Itaperuna|RJ", "Linhares"]
    /// </summary>
    public List<string>? AllowedCities { get; set; }

    /// <summary>
    /// Mensagem customizada para requisições bloqueadas.
    /// Suporta o placeholder {allowedRegions}.
    /// </summary>
    public string? BlockedMessage { get; set; }

    /// <summary>
    /// Mensagem padrão quando BlockedMessage não está configurado.
    /// </summary>
    public string? DefaultBlockedMessage { get; set; }

    /// <summary>
    /// Se true, permite acesso quando a localização não puder ser determinada (IP privado, sem cabeçalhos).
    /// Padrão é true para evitar bloqueio acidental de usuários legítimos em casos de falha de detecção.
    /// </summary>
    public bool FailOpen { get; set; } = true;
}
