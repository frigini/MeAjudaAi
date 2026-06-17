using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;

/// <summary>
/// Cliente HTTP para a API BrasilAPI.
/// </summary>
public sealed class BrasilApiCepClient(HttpClient httpClient, ILogger<BrasilApiCepClient> logger, ISerializer serializer)
{
    public async Task<Address?> GetAddressAsync(Cep cep, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"api/cep/v2/{cep.Value}";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("BrasilAPI returned status {StatusCode} for CEP {Cep}. Content: {Content}", 
                    response.StatusCode, cep.Value, errorContent);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug("BrasilAPI response for {Cep}: {Content}", cep.Value, content);
            var brasilApiResponse = serializer.Deserialize<BrasilApiCepResponse>(content);

            if (brasilApiResponse is null)
            {
                logger.LogInformation("CEP {Cep} not found in BrasilAPI (null deserialization)", cep.Value);
                return null;
            }

            var address = Address.Create(
                cep,
                brasilApiResponse.Street,
                brasilApiResponse.Neighborhood,
                brasilApiResponse.City,
                brasilApiResponse.State);

            if (address is null)
            {
                logger.LogWarning("BrasilAPI returned data for {Cep}, but Address.Create failed. Data: {@Response}", 
                    cep.Value, brasilApiResponse);
            }

            return address;
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogError("Request canceled/timed out when querying BrasilAPI for CEP {Cep}", cep.Value);
            throw new TimeoutException(
                $"BrasilAPI request timed out while querying CEP {cep.Value}",
                ex);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error querying BrasilAPI for CEP {Cep}", cep.Value);
            throw new InvalidOperationException(
                $"HTTP error querying BrasilAPI for CEP {cep.Value} (Status: {ex.StatusCode})",
                ex);
        }
        catch (Exception ex)
        {
            // Para outras exceções (parsing JSON, etc), re-lança para habilitar fallback
            logger.LogError(ex, "Unexpected error querying  BrasilAPI for CEP {Cep}", cep.Value);
            throw new InvalidOperationException(
                $"Unexpected error querying BrasilAPI for CEP {cep.Value} (may be JSON parsing or network issue)",
                ex);
        }
    }
}
