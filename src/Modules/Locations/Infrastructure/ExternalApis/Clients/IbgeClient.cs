using System.Text.Json;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
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
    /// Uses lowercase for consistent API queries and WireMock stub matching.
    /// Returns null if no exact match found (fail-closed to prevent incorrect city selection).
    /// </summary>
    public async Task<Municipio?> GetMunicipioByNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Trim input but preserve original casing for comparisons
            var trimmedCity = cityName?.Trim();
            if (string.IsNullOrEmpty(trimmedCity))
            {
                logger.LogWarning("City name is null or empty");
                return null;
            }

            // Use lowercase for IBGE API query (consistent with their search behavior)
            // This also ensures WireMock stubs work consistently
            var normalizedCity = trimmedCity.ToLowerInvariant();
            var encodedName = Uri.EscapeDataString(normalizedCity);
            var url = $"municipios?nome={encodedName}";

            logger.LogDebug("Buscando município {CityName} na API IBGE", trimmedCity);

            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("IBGE retornou status {StatusCode} para município {CityName}", response.StatusCode, cityName);
                
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
                logger.LogInformation("Município {CityName} não encontrado no IBGE", cityName);
                return null;
            }

            // Find exact match using case-insensitive comparison with the original trimmed input
            // This preserves the user's casing intent while allowing case-insensitive matching
            // Note: This does NOT remove diacritics (e.g., "Muriae" won't match "Muriaé")
            var match = municipios.FirstOrDefault(m =>
                string.Equals(m.Nome, trimmedCity, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                logger.LogWarning(
                    "Município {CityName} não encontrou match exato no IBGE. Retornando null (fail-closed). " +
                    "Resultados encontrados: {Results}",
                    trimmedCity,
                    string.Join(", ", municipios.Select(m => m.Nome)));
                return null; // Fail-closed to prevent returning incorrect city
            }

            return match;
        }
        catch (HttpRequestException ex)
        {
            // Re-throw HTTP exceptions (500, timeout, etc) to enable middleware fallback
            logger.LogError(ex, "HTTP error querying IBGE for municipality {CityName}", cityName);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            // Re-throw timeout exceptions to enable middleware fallback
            logger.LogError(ex, "Timeout querying IBGE for municipality {CityName}", cityName);
            throw;
        }
        catch (Exception ex)
        {
            // For other exceptions (JSON parsing, etc), re-throw to enable fallback
            logger.LogError(ex, "Unexpected error querying IBGE for municipality {CityName}", cityName);
            throw;
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

            logger.LogDebug("Buscando municípios da UF {UF} na API IBGE", ufSigla);

            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("IBGE retornou status {StatusCode} para UF {UF}", response.StatusCode, ufSigla);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var municipios = JsonSerializer.Deserialize<List<Municipio>>(content, SerializationDefaults.Default);

            return municipios ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar IBGE para UF {UF}", ufSigla);
            return [];
        }
    }

    /// <summary>
    /// Valida se uma cidade existe na UF especificada.
    /// </summary>
    public async Task<bool> ValidateCityInStateAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default)
    {
        try
        {
            var municipio = await GetMunicipioByNameAsync(cityName, cancellationToken);

            if (municipio is null)
            {
                return false;
            }

            var ufSigla = municipio.GetEstadoSigla();
            return string.Equals(ufSigla, stateSigla, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao validar cidade {CityName} na UF {UF}", cityName, stateSigla);
            return false;
        }
    }
}
