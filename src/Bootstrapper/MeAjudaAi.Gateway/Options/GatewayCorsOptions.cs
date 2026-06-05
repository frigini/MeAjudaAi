namespace MeAjudaAi.Gateway.Options;

/// <summary>
/// Opções de configuração para a política de CORS (Cross-Origin Resource Sharing) do Gateway.
/// </summary>
public class GatewayCorsOptions
{
    public const string SectionName = "Cors";

    public List<string> AllowedOrigins { get; } = new List<string>();
    public List<string> AllowedMethods { get; } = new List<string> { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" };
    public List<string> AllowedHeaders { get; } = new List<string> { "*" };
    public bool AllowCredentials { get; set; } = true;
    public int MaxAgeSeconds { get; set; } = 3600;
}