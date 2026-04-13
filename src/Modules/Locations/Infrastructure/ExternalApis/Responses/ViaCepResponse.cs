using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;

/// <summary>
/// Resposta da API ViaCEP.
/// Documentação: https://viacep.com.br/
/// </summary>
public sealed class ViaCepResponse
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

    /// <summary>
    /// API retorna {"erro": true} quando CEP não existe
    /// </summary>
    [JsonPropertyName("erro")]
    public bool Erro { get; set; }
}
