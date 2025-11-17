namespace MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Responses;

/// <summary>
/// Resposta da API Nominatim (OpenStreetMap).
/// Documentação: https://nominatim.org/release-docs/latest/api/Search/
/// </summary>
public sealed class NominatimResponse
{
    public string? Lat { get; set; }
    public string? Lon { get; set; }
    public string? DisplayName { get; set; }
    public string? Type { get; set; }
    public double? Importance { get; set; }
}
