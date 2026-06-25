using MeAjudaAi.ApiService.Services.Orchestration;
using MeAjudaAi.ApiService.Services.Orchestration.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.ApiService.Endpoints;

/// <summary>
/// Endpoints para receber relatórios de violação de Content Security Policy.
/// </summary>
[ExcludeFromCodeCoverage]
public static class CspReportEndpoints
{
    public static IEndpointRouteBuilder MapCspReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/csp-report")
            .WithTags("Security");

        group.MapPost("/", ReceiveCspReport)
            .WithName("ReceiveCspReport")
            .WithSummary("Recebe relatórios de violação de Content Security Policy")
            .WithDescription("Recebe relatórios CSP de navegadores. Limite de 64KB.")
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> ReceiveCspReport(
        HttpContext context,
        [FromServices] ICspReportService cspReportService)
    {
        if (context.Request.ContentLength > 64 * 1024)
        {
            return TypedResults.BadRequest(new { error = "CSP report too large. Maximum size is 64KB." });
        }

        using var reader = new StreamReader(context.Request.Body);
        var reportJson = await reader.ReadToEndAsync();

        var result = cspReportService.ProcessReport(reportJson);

        if (result.IsFailure)
        {
            return TypedResults.BadRequest(result.Error);
        }

        return TypedResults.NoContent();
    }
}
