namespace MeAjudaAi.Contracts.Configuration;

/// <summary>
/// Configuração do Keycloak para autenticação OIDC.
/// </summary>
public sealed record KeycloakConfiguration
{
    /// <summary>
    /// URL do realm Keycloak (authority).
    /// Exemplo: https://auth.meajudaai.com/realms/meajudaai
    /// </summary>
    public string Authority { get; init; } = string.Empty;

    /// <summary>
    /// ID do cliente registrado no Keycloak.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Tipo de resposta OAuth2 (geralmente "code" para PKCE).
    /// </summary>
    public string ResponseType { get; init; } = "code";

    /// <summary>
    /// Escopos solicitados (space-separated).
    /// </summary>
    public string Scope { get; init; } = "openid profile email";

    /// <summary>
    /// URL de redirecionamento após logout.
    /// </summary>
    public string PostLogoutRedirectUri { get; init; } = string.Empty;
}
