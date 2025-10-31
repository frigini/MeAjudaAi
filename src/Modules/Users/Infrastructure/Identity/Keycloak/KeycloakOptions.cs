namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;

public class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    public string BaseUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;

    public bool RequireHttpsMetadata { get; set; } = true;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

    public string AuthorityUrl => $"{BaseUrl}/realms/{Realm}";
    public string TokenUrl => $"{AuthorityUrl}/protocol/openid-connect/token";
    public string UsersUrl => $"{BaseUrl}/admin/realms/{Realm}/users";
}
