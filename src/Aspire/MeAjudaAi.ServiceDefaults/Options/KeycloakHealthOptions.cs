namespace MeAjudaAi.ServiceDefaults.Options;

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
