using System.Text.Json;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients.Interfaces;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;

/// <summary>
/// Cliente HTTP para a API IBGE Localidades.
/// Documentação: https://servicodados.ibge.gov.br/api/docs/localidades
/// </summary>
public sealed class IbgeClient(HttpClient httpClient, ILogger<IbgeClient> logger) : IIbgeClient
{
    /// <summary>
    /// Busca um município por nome usando query parameter.
    /// Exemplo: "Muriaé" → "/municipios?nome=muriaé"
    /// Usa lowercase para consultas consistentes à API e matching de stubs WireMock.
    /// Retorna null se nenhum match exato for encontrado (fail-closed para prevenir seleção incorreta de cidade).
    /// </summary>
    public async Task<Municipio?> GetMunicipioByNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Remove espaços mas preserva capitalização original para comparações
            var trimmedCity = cityName?.Trim();
            if (string.IsNullOrEmpty(trimmedCity))
            {
                logger.LogWarning("City name is null or empty");
                return null;
            }

            // Usa lowercase para consulta à API IBGE (consistente com comportamento de busca)
            // Isso também garante que stubs WireMock funcionem consistentemente
            var normalizedCity = trimmedCity.ToLowerInvariant();
            var encodedName = Uri.EscapeDataString(normalizedCity);
            var url = $"municipios?nome={encodedName}";

            logger.LogDebug("Querying IBGE API for municipality {CityName}", trimmedCity);

            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("IBGE returned status {StatusCode} for municipality {CityName}", response.StatusCode, cityName);

                // Throw exception for HTTP errors to enable middleware fallback to simple validation
                // This ensures fail-open behavior when IBGE service is unavailable
                throw new HttpRequestException(
                    $"IBGE API returned {response.StatusCode} for municipality {cityName}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // A API retorna array quando busca por nome
            var municipios = JsonSerializer.Deserialize<List<Municipio>>(content, SerializationDefaults.Default);

            if (municipios is null || municipios.Count == 0)
            {
                logger.LogInformation("Municipality {CityName} not found in IBGE", cityName);
                return null;
            }

            // Encontra match exato usando comparação case-insensitive com o input original trimmed
            // Isso preserva a intenção de capitalização do usuário enquanto permite matching case-insensitive
            // Nota: Isso NÃO remove diacríticos (ex: "Muriae" não fará match com "Muriaé")
            var match = municipios.FirstOrDefault(m =>
                string.Equals(m.Nome, trimmedCity, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                logger.LogWarning(
                    "Municipality {CityName} did not find exact match in IBGE. Returning null (fail-closed). " +
                    "Found results: {Results}",
                    trimmedCity,
                    string.Join(", ", municipios.Select(m => m.Nome)));
                return null; // Fail-closed para prevenir retorno de cidade incorreta
            }

            return match;
        }
        catch (HttpRequestException ex)
        {
            // Re-lança exceções HTTP (500, timeout, etc) para habilitar fallback do middleware
            logger.LogError(ex, "HTTP error querying IBGE for municipality {CityName}", cityName);
            throw new InvalidOperationException(
                $"HTTP error querying IBGE API for municipality '{cityName}' (Status: {ex.StatusCode})",
                ex);
        }
        catch (TaskCanceledException ex) when (ex != null)
        {
            // Re-lança exceções de timeout para habilitar fallback do middleware
            logger.LogError(ex, "Timeout querying IBGE for municipality {CityName}", cityName);
            throw new TimeoutException(
                $"IBGE API request timed out while querying municipality '{cityName}'",
                ex);
        }
        catch (Exception ex)
        {
            // Para outras exceções (parsing JSON, etc), re-lança para habilitar fallback
            logger.LogError(ex, "Unexpected error querying IBGE for municipality {CityName}", cityName);
            throw new InvalidOperationException(
                $"Unexpected error querying IBGE API for municipality '{cityName}' (may be JSON parsing or network issue)",
                ex);
        }
    }

    /// <summary>
    /// Busca todos os municípios de uma UF.
    /// </summary>
    public async Task<List<Municipio>> GetMunicipiosByUFAsync(string ufSigla, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"estados/{ufSigla.ToUpperInvariant()}/municipios";

            logger.LogDebug("Querying IBGE API for municipalities in state {UF}", ufSigla);

            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("IBGE returned status {StatusCode} for state {UF}", response.StatusCode, ufSigla);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var municipios = JsonSerializer.Deserialize<List<Municipio>>(content, SerializationDefaults.Default);

            return municipios ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error querying IBGE for state {UF}", ufSigla);
            return [];
        }
    }

    /// <summary>
    /// Valida se uma cidade existe na UF especificada.
    /// </summary>
    public async Task<bool> ValidateCityInStateAsync(string city, string state, CancellationToken cancellationToken = default)
    {
        try
        {
            var municipio = await GetMunicipioByNameAsync(city, cancellationToken);

            if (municipio is null)
            {
                return false;
            }

            var ufSigla = municipio.GetEstadoSigla();
            return string.Equals(ufSigla, state, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating city {CityName} in state {UF}", city, state);
            return false;
        }
    }
}
