namespace MeAjudaAi.ServiceDefaults.Options;

/// <summary>
/// Opções de configuração para health check do serviço de geolocalização
/// </summary>
public class GeolocationHealthOptions
{
    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = string.Empty;
    public string HealthEndpointPath { get; set; } = "/health";
    public int TimeoutSeconds { get; set; } = 5;
}
