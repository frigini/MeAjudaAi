namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak.Models;

public class KeycloakCreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool EmailVerified { get; set; }
    public KeycloakCredential[] Credentials { get; set; } = [];
}