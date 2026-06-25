using MeAjudaAi.ApiService.Endpoints.Models;
using MeAjudaAi.ApiService.Services.Orchestration.Interfaces;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.ApiService.Services.Orchestration;

/// <summary>
/// Serviço que processa relatórios de violações de CSP (Content Security Policy).
/// </summary>
public sealed class CspReportService(
    ISerializer serializer,
    ILogger<CspReportService> logger) : ICspReportService
{
    public Result ProcessReport(string reportJson)
    {
        if (string.IsNullOrWhiteSpace(reportJson))
            return Result.Failure(new Error("Report is empty", 400));

        CspViolationReport? report;
        try
        {
            report = serializer.Deserialize<CspViolationReport>(reportJson);
        }
        catch (System.Text.Json.JsonException)
        {
            return Result.Failure(new Error("Invalid CSP report", 400));
        }

        if (report?.CspReport != null)
        {
            logger.LogWarning(
                "CSP Violation: {DocumentUri} blocked {ViolatedDirective} from {BlockedUri}. Original Policy: {OriginalPolicy}",
                report.CspReport.DocumentUri,
                report.CspReport.ViolatedDirective,
                report.CspReport.BlockedUri,
                report.CspReport.OriginalPolicy);
        }

        return Result.Success();
    }
}
