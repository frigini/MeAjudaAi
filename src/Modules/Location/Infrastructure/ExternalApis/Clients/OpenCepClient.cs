using System.Text.Json;
using MeAjudaAi.Modules.Location.Domain.ValueObjects;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Responses;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;

/// <summary>
/// Cliente HTTP para a API OpenCEP.
/// Endpoint: https://opencep.com/v1/{cep}
/// </summary>
public sealed class OpenCepClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenCepClient> _logger;

    public OpenCepClient(HttpClient httpClient, ILogger<OpenCepClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Address?> GetAddressAsync(Cep cep, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://opencep.com/v1/{cep.Value}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenCEP retornou status {StatusCode} para CEP {Cep}", response.StatusCode, cep.Value);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var openCepResponse = JsonSerializer.Deserialize<OpenCepResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (openCepResponse is null)
            {
                _logger.LogInformation("CEP {Cep} não encontrado no OpenCEP", cep.Value);
                return null;
            }

            return Address.Create(
                cep,
                openCepResponse.Logradouro,
                openCepResponse.Bairro,
                openCepResponse.Localidade,
                openCepResponse.Uf,
                openCepResponse.Complemento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar OpenCEP para CEP {Cep}", cep.Value);
            return null;
        }
    }
}
