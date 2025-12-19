namespace MeAjudaAi.ApiService.Options;

/// <summary>
/// Opções de segurança específicas por ambiente
/// </summary>
public class SecurityOptions
{
    public bool EnforceHttps { get; set; }
    public bool EnableStrictTransportSecurity { get; set; }
    public IReadOnlyList<string> AllowedHosts { get; set; } = [];
}
