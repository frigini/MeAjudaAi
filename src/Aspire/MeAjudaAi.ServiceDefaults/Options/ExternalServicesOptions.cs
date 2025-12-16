namespace MeAjudaAi.ServiceDefaults.Options;

/// <summary>
/// Opções de configuração para health checks de serviços externos
/// </summary>
public class ExternalServicesOptions
{
    public const string SectionName = "ExternalServices";

    public KeycloakHealthOptions Keycloak { get; set; } = new();
    public GeolocationHealthOptions Geolocation { get; set; } = new();
}

/// <summary>
/// Opções de configuração para health check do Keycloak
/// </summary>
public class KeycloakHealthOptions
{
    public bool Enabled { get; set; } = true;
    // Keycloak v25+ expõe endpoints de health na porta de gerenciamento 9000 por padrão
    public string BaseUrl { get; set; } = "http://localhost:9000";
    // Use /health/ready para probes de prontidão no Kubernetes; /health/live para liveness
    public string HealthEndpointPath { get; set; } = "/health/ready";
    public int TimeoutSeconds { get; set; } = 5;
}

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

