using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Authorization.Keycloak.Models;

/// <summary>
/// Configuração para integração com o servidor Keycloak.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class KeycloakConfiguration
{
    /// <summary>
    /// URL base do servidor Keycloak (ex: https://keycloak.example.com).
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Nome do realm Keycloak onde os usuários e roles estão definidos.
    /// </summary>
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// Client ID administrativo usado para autenticação no Keycloak.
    /// </summary>
    public string AdminClientId { get; set; } = string.Empty;

    /// <summary>
    /// Segredo do client administrativo usado para autenticação no Keycloak.
    /// </summary>
    public string AdminClientSecret { get; set; } = string.Empty;
}
