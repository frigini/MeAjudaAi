namespace MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Responses;

/// <summary>
/// Resposta da API ViaCEP.
/// Documentação: https://viacep.com.br/
/// </summary>
public sealed class ViaCepResponse
{
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Localidade { get; set; }
    public string? Uf { get; set; }
    public bool Erro { get; set; } // API retorna {"erro": true} quando CEP não existe
}

/// <summary>
/// Resposta da API BrasilAPI.
/// Documentação: https://brasilapi.com.br/docs#tag/CEP
/// </summary>
public sealed class BrasilApiCepResponse
{
    public string? Cep { get; set; }
    public string? Street { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}

/// <summary>
/// Resposta da API OpenCEP.
/// Documentação: https://opencep.com/
/// </summary>
public sealed class OpenCepResponse
{
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Localidade { get; set; }
    public string? Uf { get; set; }
}
