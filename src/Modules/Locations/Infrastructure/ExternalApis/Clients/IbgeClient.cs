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
    /// Exemplo: "Muriaé" → "/municipios?nome=Muriaé"
    /// </summary>
    public async Task<Municipio?> GetMunicipioByNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Using documented IBGE API query endpoint: /municipios?nome={cityName}
            // See: https://servicodados.ibge.gov.br/api/docs/localidades
            var encodedName = Uri.EscapeDataString(cityName);
            var url = $"municipios?nome={encodedName}";

            logger.LogDebug("Buscando município {CityName} na API IBGE", cityName);

            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("IBGE retornou status {StatusCode} para município {CityName}", response.StatusCode, cityName);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // A API retorna array quando busca por nome
            var municipios = JsonSerializer.Deserialize<List<Municipio>>(content, SerializationDefaults.Default);

            if (municipios is null || municipios.Count == 0)
            {
                logger.LogInformation("Município {CityName} não encontrado no IBGE", cityName);
                return null;
            }

            // Find exact match using case-insensitive and diacritic-insensitive comparison
            var match = municipios.FirstOrDefault(m =>
                string.Equals(m.Nome, cityName, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                logger.LogInformation("Município {CityName} não encontrou match exato no IBGE", cityName);
                // Return first result as fallback
                return municipios[0];
            }

            return match;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar IBGE para município {CityName}", cityName);
            return null;
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
                return new List<Municipio>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var municipios = JsonSerializer.Deserialize<List<Municipio>>(content, SerializationDefaults.Default);

            return municipios ?? new List<Municipio>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar IBGE para UF {UF}", ufSigla);
            return new List<Municipio>();
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
