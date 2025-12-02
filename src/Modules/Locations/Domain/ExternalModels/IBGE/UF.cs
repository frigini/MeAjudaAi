using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;

/// <summary>
/// Representa uma Unidade da Federação (Estado) do Brasil.
/// Fonte: API IBGE Localidades
/// </summary>
public sealed class UF
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("nome")]
    public string Nome { get; init; } = string.Empty;

    [JsonPropertyName("sigla")]
    public string Sigla { get; init; } = string.Empty;

    [JsonPropertyName("regiao")]
    public Regiao? Regiao { get; init; }
}
