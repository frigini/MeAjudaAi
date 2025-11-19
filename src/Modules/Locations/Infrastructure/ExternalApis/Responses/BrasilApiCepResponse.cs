namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;

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
