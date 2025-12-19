using System.Text.Json;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;

/// <summary>
/// Cliente HTTP para a API ViaCEP.
/// </summary>
public sealed class ViaCepClient(HttpClient httpClient, ILogger<ViaCepClient> logger)
{

    public async Task<Address?> GetAddressAsync(Cep cep, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"ws/{cep.Value}/json/";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("ViaCEP returned status {StatusCode} for CEP {Cep}", response.StatusCode, cep.Value);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var viaCepResponse = JsonSerializer.Deserialize<ViaCepResponse>(content, SerializationDefaults.Default);

            if (viaCepResponse is null || viaCepResponse.Erro)
            {
                logger.LogInformation("CEP {Cep} not found in ViaCEP", cep.Value);
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
            logger.LogError(ex, "Error querying ViaCEP for CEP {Cep}", cep.Value);
            return null;
        }
    }
}
