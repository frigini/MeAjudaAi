namespace MeAjudaAi.Contracts.Configuration;

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
