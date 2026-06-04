using System.Text.Json.Serialization;

namespace MeAjudaAi.Shared.Authorization.Keycloak.Models;

public sealed class KeycloakConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string AdminClientId { get; set; } = string.Empty;
    public string AdminClientSecret { get; set; } = string.Empty;
}
