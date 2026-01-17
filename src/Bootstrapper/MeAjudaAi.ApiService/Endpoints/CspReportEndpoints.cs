using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeAjudaAi.ApiService.Endpoints;

/// <summary>
/// Endpoint para receber relatórios de violação de CSP.
/// Permite monitorar tentativas de XSS e outras violações de segurança.
/// </summary>
public static class CspReportEndpoints
{
    public static IEndpointRouteBuilder MapCspReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/csp-report")
            .WithTags("Security");

        group.MapPost("/", ReceiveCspReport)
            .WithName("ReceiveCspReport")
            .WithSummary("Recebe relatórios de violação de Content Security Policy")
            .AllowAnonymous(); // Deve ser anônimo para receber relatórios do browser

        return endpoints;
    }

    /// <summary>
    /// Recebe e registra violações de CSP.
    /// </summary>
    private static async Task<IResult> ReceiveCspReport(
        HttpContext context,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            using var reader = new StreamReader(context.Request.Body);
            var reportJson = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(reportJson))
            {
                return Results.BadRequest("Relatório vazio");
            }

            // Analisar o relatório CSP
            var report = JsonSerializer.Deserialize<CspViolationReport>(reportJson);

            if (report?.CspReport != null)
            {
                logger.LogWarning(
                    "CSP Violation: {DocumentUri} blocked {ViolatedDirective} from {BlockedUri}. Original Policy: {OriginalPolicy}",
                    report.CspReport.DocumentUri,
                    report.CspReport.ViolatedDirective,
                    report.CspReport.BlockedUri,
                    report.CspReport.OriginalPolicy);

                // Em produção, você pode querer armazenar isso em um sistema de monitoramento
                // ou enviar alertas se houver muitas violações
            }

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing CSP report");
            return Results.StatusCode(500);
        }
    }
}

/// <summary>
/// Modelo para relatório de violação CSP enviado pelo browser.
/// </summary>
public class CspViolationReport
{
    [JsonPropertyName("csp-report")]
    public CspReportDetails? CspReport { get; set; }
}

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
