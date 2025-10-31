namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak.Models;

public class KeycloakCredential
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Temporary { get; set; }
}