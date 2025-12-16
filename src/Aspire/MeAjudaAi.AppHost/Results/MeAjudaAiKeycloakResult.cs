namespace MeAjudaAi.AppHost.Results;

/// <summary>
/// Resultado da configuração do Keycloak
/// </summary>
public sealed class MeAjudaAiKeycloakResult
{
    /// <summary>
    /// Referência ao container do Keycloak
    /// </summary>
    public required IResourceBuilder<KeycloakResource> Keycloak { get; init; }

    /// <summary>
    /// URL base do Keycloak para autenticação
    /// </summary>
    public required string AuthUrl { get; init; }

    /// <summary>
    /// URL de administração do Keycloak
    /// </summary>
    public required string AdminUrl { get; init; }
}
