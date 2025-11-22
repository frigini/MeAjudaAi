using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;

/// <summary>
/// Representa um Município brasileiro completo com hierarquia geográfica.
/// Fonte: API IBGE Localidades
/// </summary>
public sealed class Municipio
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("nome")]
    public string Nome { get; init; } = string.Empty;

    [JsonPropertyName("microrregiao")]
    public Microrregiao? Microrregiao { get; init; }

    /// <summary>
    /// Obtém a UF do município através da hierarquia geográfica.
    /// </summary>
    public UF? GetUF() => Microrregiao?.Mesorregiao?.UF;

    /// <summary>
    /// Obtém a sigla do estado (ex: "MG", "RJ", "ES").
    /// </summary>
    public string? GetEstadoSigla() => GetUF()?.Sigla;

    /// <summary>
    /// Obtém o nome completo formatado: "Cidade - UF" (ex: "Muriaé - MG").
    /// </summary>
    public string GetNomeCompleto() => $"{Nome} - {GetEstadoSigla() ?? "??"}";
}
