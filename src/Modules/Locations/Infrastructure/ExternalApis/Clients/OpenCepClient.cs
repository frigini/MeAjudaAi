using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
                logger.LogWarning("OpenCEP retornou status {StatusCode} para CEP {Cep}", response.StatusCode, cep.Value);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var openCepResponse = JsonSerializer.Deserialize<OpenCepResponse>(content, SerializationDefaults.Default);

            if (openCepResponse is null)
            {
                logger.LogInformation("CEP {Cep} não encontrado no OpenCEP", cep.Value);
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
            logger.LogError(ex, "Erro ao consultar OpenCEP para CEP {Cep}", cep.Value);
            return null;
        }
    }
}
