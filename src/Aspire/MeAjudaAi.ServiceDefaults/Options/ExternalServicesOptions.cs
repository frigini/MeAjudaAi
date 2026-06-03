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
