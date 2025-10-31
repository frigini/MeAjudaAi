using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.Shared.Authorization.Keycloak;

/// <summary>
/// Opções de configuração para integração com Keycloak.
/// </summary>
public sealed class KeycloakPermissionOptions
{
    /// <summary>
    /// URL base do servidor Keycloak.
    /// </summary>
    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Nome do realm do Keycloak.
    /// </summary>
    [Required]
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// Client ID para autenticação administrativa.
    /// </summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client secret para autenticação administrativa.
    /// </summary>
    [Required]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Username do admin para operações administrativas.
    /// </summary>
    [Required]
    public string AdminUsername { get; set; } = string.Empty;

    /// <summary>
    /// Password do admin para operações administrativas.
    /// </summary>
    [Required]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>
    /// Timeout para requisições HTTP em segundos.
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Duração do cache de permissões em minutos.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Se deve validar certificados SSL (usar false apenas em desenvolvimento).
    /// </summary>
    public bool ValidateSslCertificate { get; set; } = true;
}
