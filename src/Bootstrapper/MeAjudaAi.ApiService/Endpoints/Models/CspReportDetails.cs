using System.Text.Json.Serialization;

namespace MeAjudaAi.ApiService.Endpoints.Models;

/// <summary>
/// Detalhes da violação CSP.
/// </summary>
public class CspReportDetails
{
    [JsonPropertyName("document-uri")]
    public string? DocumentUri { get; set; }
    
    [JsonPropertyName("referrer")]
    public string? Referrer { get; set; }
    
    [JsonPropertyName("blocked-uri")]
    public string? BlockedUri { get; set; }
    
    [JsonPropertyName("violated-directive")]
    public string? ViolatedDirective { get; set; }
    
    [JsonPropertyName("effective-directive")]
    public string? EffectiveDirective { get; set; }
    
    [JsonPropertyName("original-policy")]
    public string? OriginalPolicy { get; set; }
    
    [JsonPropertyName("disposition")]
    public string? Disposition { get; set; }
    
    [JsonPropertyName("status-code")]
    public int StatusCode { get; set; }
}
