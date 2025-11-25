using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;

/// <summary>
/// Representa uma Mesorregião (divisão geográfica regional) do Brasil.
/// Fonte: API IBGE Localidades
/// </summary>
public sealed class Mesorregiao
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("nome")]
    public string Nome { get; init; } = string.Empty;

    [JsonPropertyName("UF")]
    public UF? UF { get; init; }
}
