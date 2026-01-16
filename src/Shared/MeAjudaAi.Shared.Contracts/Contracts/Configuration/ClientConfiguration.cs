namespace MeAjudaAi.Shared.Contracts.Configuration;

/// <summary>
/// Configuração do cliente Blazor WASM.
/// Contém apenas informações não-sensíveis necessárias para o frontend.
/// </summary>
public sealed record ClientConfiguration
{
    /// <summary>
    /// URL base da API backend.
    /// </summary>
    public string ApiBaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Configurações do Keycloak OIDC.
    /// </summary>
    public KeycloakConfiguration Keycloak { get; init; } = new();

    /// <summary>
    /// URLs de recursos externos (se houver).
    /// </summary>
    public ExternalResources External { get; init; } = new();

    /// <summary>
    /// Configurações de feature flags (opcional).
    /// </summary>
    public FeatureFlags Features { get; init; } = new();
}

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

/// <summary>
/// URLs de recursos externos.
/// </summary>
public sealed record ExternalResources
{
    /// <summary>
    /// URL da documentação/help center (se houver).
    /// </summary>
    public string? DocumentationUrl { get; init; }

    /// <summary>
    /// URL do suporte/support portal (se houver).
    /// </summary>
    public string? SupportUrl { get; init; }
}

/// <summary>
/// Feature flags para habilitar/desabilitar funcionalidades no frontend.
/// </summary>
public sealed record FeatureFlags
{
    /// <summary>
    /// Habilita Redux DevTools (somente em desenvolvimento).
    /// </summary>
    public bool EnableReduxDevTools { get; init; }

    /// <summary>
    /// Habilita mode de debug/diagnóstico.
    /// </summary>
    public bool EnableDebugMode { get; init; }
}
