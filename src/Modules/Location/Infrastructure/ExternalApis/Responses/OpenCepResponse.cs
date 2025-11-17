namespace MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Responses;

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
