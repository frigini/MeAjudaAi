using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;

/// <summary>
/// Resposta da API Nominatim (OpenStreetMap).
/// Documentação: https://nominatim.org/release-docs/latest/api/Search/
/// </summary>
public sealed class NominatimResponse
{
    [JsonPropertyName("lat")]
    public string? Lat { get; set; }

    [JsonPropertyName("lon")]
    public string? Lon { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    public string? Type { get; set; }
    public double? Importance { get; set; }

    [JsonPropertyName("address")]
    public NominatimAddress? Address { get; set; }
}

