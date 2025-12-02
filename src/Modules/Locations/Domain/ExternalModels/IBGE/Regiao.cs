using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;

/// <summary>
/// Representa uma região do Brasil (Norte, Nordeste, Sudeste, Sul, Centro-Oeste).
/// Fonte: API IBGE Localidades
/// </summary>
public sealed class Regiao
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("nome")]
    public string Nome { get; init; } = string.Empty;

    [JsonPropertyName("sigla")]
    public string Sigla { get; init; } = string.Empty;
}
