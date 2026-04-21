using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;

/// <summary>
/// Resposta da API BrasilAPI.
/// Documentação: https://brasilapi.com.br/docs#tag/CEP
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class BrasilApiCepResponse
{
    [JsonPropertyName("cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("neighborhood")]
    public string? Neighborhood { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}
