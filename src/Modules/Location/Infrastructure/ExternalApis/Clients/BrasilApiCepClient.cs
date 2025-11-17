using System.Text.Json;
using MeAjudaAi.Modules.Location.Domain.ValueObjects;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Responses;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;

/// <summary>
/// Cliente HTTP para a API BrasilAPI.
/// Endpoint: https://brasilapi.com.br/api/cep/v2/{cep}
/// </summary>
public sealed class BrasilApiCepClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BrasilApiCepClient> _logger;

    public BrasilApiCepClient(HttpClient httpClient, ILogger<BrasilApiCepClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Address?> GetAddressAsync(Cep cep, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://brasilapi.com.br/api/cep/v2/{cep.Value}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BrasilAPI retornou status {StatusCode} para CEP {Cep}", response.StatusCode, cep.Value);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var brasilApiResponse = JsonSerializer.Deserialize<BrasilApiCepResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (brasilApiResponse is null)
            {
                _logger.LogInformation("CEP {Cep} não encontrado no BrasilAPI", cep.Value);
                return null;
            }

            return Address.Create(
                cep,
                brasilApiResponse.Street,
                brasilApiResponse.Neighborhood,
                brasilApiResponse.City,
                brasilApiResponse.State);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar BrasilAPI para CEP {Cep}", cep.Value);
            return null;
        }
    }
}
