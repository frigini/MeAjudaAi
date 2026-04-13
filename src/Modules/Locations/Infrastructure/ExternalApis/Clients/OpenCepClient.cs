using System.Text.Json;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;

/// <summary>
/// Cliente HTTP para a API OpenCEP.
/// </summary>
public sealed class OpenCepClient(HttpClient httpClient, ILogger<OpenCepClient> logger)
{

    public async Task<Address?> GetAddressAsync(Cep cep, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"v1/{cep.Value}";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("OpenCEP returned status {StatusCode} for CEP {Cep}. Content: {Content}", 
                    response.StatusCode, cep.Value, errorContent);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug("OpenCEP response for {Cep}: {Content}", cep.Value, content);
            var openCepResponse = JsonSerializer.Deserialize<OpenCepResponse>(content, SerializationDefaults.Api);

            if (openCepResponse is null)
            {
                logger.LogInformation("CEP {Cep} not found in OpenCEP", cep.Value);
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
            logger.LogError(ex, "Error querying OpenCEP for CEP {Cep}", cep.Value);
            return null;
        }
    }
}
