using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;

public sealed class NominatimAddress
{
    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("town")]
    public string? Town { get; set; }

    [JsonPropertyName("village")]
    public string? Village { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
    
    [JsonPropertyName("municipality")]
    public string? Municipality { get; set; }
}
