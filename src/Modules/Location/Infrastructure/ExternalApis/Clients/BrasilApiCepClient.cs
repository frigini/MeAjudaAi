using MeAjudaAi.Modules.Location.Domain.ValueObjects;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;

/// <summary>
/// Cliente HTTP para a API BrasilAPI.
/// </summary>
public sealed class BrasilApiCepClient(HttpClient httpClient, ILogger<BrasilApiCepClient> logger)
{
    public async Task<Address?> GetAddressAsync(Cep cep, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"api/cep/v2/{cep.Value}";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("BrasilAPI retornou status {StatusCode} para CEP {Cep}", response.StatusCode, cep.Value);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var brasilApiResponse = JsonSerializer.Deserialize<BrasilApiCepResponse>(content, SerializationDefaults.Default);

            if (brasilApiResponse is null)
            {
                logger.LogInformation("CEP {Cep} não encontrado no BrasilAPI", cep.Value);
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
            logger.LogError(ex, "Erro ao consultar BrasilAPI para CEP {Cep}", cep.Value);
            return null;
        }
    }
}
