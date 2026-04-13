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
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("ViaCEP returned status {StatusCode} for CEP {Cep}. Content: {Content}", 
                    response.StatusCode, cep.Value, errorContent);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug("ViaCEP response for {Cep}: {Content}", cep.Value, content);
            var viaCepResponse = JsonSerializer.Deserialize<ViaCepResponse>(content, SerializationDefaults.Api);

            if (viaCepResponse is null)
            {
                logger.LogInformation("CEP {Cep} not found in ViaCEP (null response)", cep.Value);
                return null;
            }

            if (viaCepResponse.Erro)
            {
                logger.LogInformation("CEP {Cep} not found in ViaCEP (erro=true)", cep.Value);
                return null;
            }

            var address = Address.Create(
                cep,
                viaCepResponse.Logradouro,
                viaCepResponse.Bairro,
                viaCepResponse.Localidade,
                viaCepResponse.Uf,
                viaCepResponse.Complemento);

            if (address is null)
            {
                logger.LogWarning("ViaCEP returned data for CEP {Cep}, but Address.Create failed. Data: {@Response}", 
                    cep.Value, viaCepResponse);
            }

            return address;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error querying ViaCEP for CEP {Cep}", cep.Value);
            return null;
        }
    }
}
