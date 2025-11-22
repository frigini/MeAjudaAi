using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;

/// <summary>
/// Representa uma Microrregião (subdivisão das mesorregiões) do Brasil.
/// Fonte: API IBGE Localidades
/// </summary>
public sealed class Microrregiao
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("nome")]
    public string Nome { get; init; } = string.Empty;

    [JsonPropertyName("mesorregiao")]
    public Mesorregiao? Mesorregiao { get; init; }
}
