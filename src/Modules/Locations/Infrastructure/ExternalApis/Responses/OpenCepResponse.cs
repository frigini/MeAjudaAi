using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;

/// <summary>
/// Resposta da API OpenCEP.
/// Documentação: https://opencep.com/
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class OpenCepResponse
{
    [JsonPropertyName("cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("logradouro")]
    public string? Logradouro { get; set; }

    [JsonPropertyName("complemento")]
    public string? Complemento { get; set; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; set; }

    [JsonPropertyName("localidade")]
    public string? Localidade { get; set; }

    [JsonPropertyName("uf")]
    public string? Uf { get; set; }
}
