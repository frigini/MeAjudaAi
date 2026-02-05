using System.Text.Json.Serialization;

namespace MeAjudaAi.Contracts.Models;

/// <summary>
/// Representa a localização detectada do usuário.
/// </summary>
public class UserLocation
{
    /// <summary>
    /// Nome da cidade.
    /// </summary>
    /// <example>São Paulo</example>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// Sigla do estado (UF).
    /// </summary>
    /// <example>SP</example>
    [JsonPropertyName("state")]
    public string? State { get; set; }
}
