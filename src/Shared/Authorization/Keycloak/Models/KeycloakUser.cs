using System.Diagnostics.CodeAnalysis;

using System.Text.Json.Serialization;

namespace MeAjudaAi.Shared.Authorization.Keycloak.Models;

/// <summary>
/// Representação de um usuário no Keycloak.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class KeycloakUser
{
    /// <summary>
    /// Identificador único do usuário no Keycloak.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nome de usuário.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Endereço de e-mail do usuário.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Indica se o usuário está habilitado.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}
