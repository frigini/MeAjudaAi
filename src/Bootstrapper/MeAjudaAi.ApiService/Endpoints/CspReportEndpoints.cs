using MeAjudaAi.ApiService.Services.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.ApiService.Endpoints;

[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class CspReportEndpoints
{
    public static IEndpointRouteBuilder MapCspReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/csp-report")
            .WithTags("Security");

        group.MapPost("/", ReceiveCspReport)
            .WithName("ReceiveCspReport")
            .WithSummary("Recebe relatórios de violação de Content Security Policy")
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> ReceiveCspReport(
        HttpContext context,
        [FromServices] ICspReportService cspReportService)
    {
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
