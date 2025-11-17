using System.Text.Json;
using MeAjudaAi.Modules.Location.Domain.ValueObjects;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Responses;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;

/// <summary>
/// Cliente HTTP para a API ViaCEP.
/// Endpoint: https://viacep.com.br/ws/{cep}/json/
/// </summary>
public sealed class ViaCepClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepClient> _logger;

    public ViaCepClient(HttpClient httpClient, ILogger<ViaCepClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Address?> GetAddressAsync(Cep cep, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://viacep.com.br/ws/{cep.Value}/json/";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ViaCEP retornou status {StatusCode} para CEP {Cep}", response.StatusCode, cep.Value);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var viaCepResponse = JsonSerializer.Deserialize<ViaCepResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (viaCepResponse is null || viaCepResponse.Erro)
            {
                _logger.LogInformation("CEP {Cep} não encontrado no ViaCEP", cep.Value);
                return null;
            }

            return Address.Create(
                cep,
                viaCepResponse.Logradouro,
                viaCepResponse.Bairro,
                viaCepResponse.Localidade,
                viaCepResponse.Uf,
                viaCepResponse.Complemento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar ViaCEP para CEP {Cep}", cep.Value);
            return null;
        }
    }
}
