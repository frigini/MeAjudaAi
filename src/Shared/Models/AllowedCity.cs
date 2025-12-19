using System.Text.Json.Serialization;

namespace MeAjudaAi.Shared.Models;

/// <summary>
/// Representa uma cidade permitida para acesso ao serviço.
/// </summary>
public class AllowedCity
{
    /// <summary>
    /// Nome da cidade.
    /// </summary>
    /// <example>Muriaé</example>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Sigla do estado (UF).
    /// </summary>
    /// <example>MG</example>
    [JsonPropertyName("state")]
    public required string State { get; set; }

    /// <summary>
    /// Código IBGE do município (7 dígitos).
    /// </summary>
    /// <example>3143906</example>
    [JsonPropertyName("ibgeCode")]
    public string? IbgeCode { get; set; }
}
