using System.Globalization;
using System.Text;
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
    /// Busca um município por nome (URL-friendly).
    /// Exemplo: "Muriaé" → "/municipios/muriae"
    /// </summary>
    public async Task<Municipio?> GetMunicipioByNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedName = NormalizeCityName(cityName);
            var url = $"municipios/{normalizedName}";

            logger.LogDebug("Buscando município {CityName} (normalizado: {NormalizedName}) na API IBGE", cityName, normalizedName);

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

            // Retornar o primeiro resultado (geralmente único para nomes normalizados)
            return municipios[0];
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

    /// <summary>
    /// Normaliza o nome da cidade para formato URL-friendly do IBGE.
    /// Remove acentos, converte para minúsculas e substitui espaços por hifens.
    /// Exemplo: "Muriaé" → "muriae", "Rio de Janeiro" → "rio-de-janeiro"
    /// </summary>
    private static string NormalizeCityName(string cityName)
    {
        // Remover acentos
        var normalizedString = cityName.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Converter para lowercase e substituir espaços por hifens
        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant()
            .Replace(' ', '-');
    }
}
