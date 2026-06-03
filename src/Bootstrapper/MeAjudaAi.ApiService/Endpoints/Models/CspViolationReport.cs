using System.Text.Json.Serialization;

namespace MeAjudaAi.ApiService.Endpoints.Models;

/// <summary>
/// Modelo para relatório de violação CSP enviado pelo browser.
/// </summary>
public class CspViolationReport
{
    [JsonPropertyName("csp-report")]
    public CspReportDetails? CspReport { get; set; }
}
