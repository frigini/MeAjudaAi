namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;

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

    /// <summary>
    /// API retorna {"erro": true} quando CEP não existe
    /// </summary>
    public bool Erro { get; set; }
}
