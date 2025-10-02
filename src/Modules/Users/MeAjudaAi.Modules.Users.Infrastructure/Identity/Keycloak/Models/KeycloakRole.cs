namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak.Models;

public class KeycloakRole
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Composite { get; set; }
    public string? ContainerId { get; set; }
}