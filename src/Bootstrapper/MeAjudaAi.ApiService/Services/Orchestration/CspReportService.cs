using System.Text.Json;
using MeAjudaAi.ApiService.Endpoints.Models;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.ApiService.Services.Orchestration;

public interface ICspReportService
{
    Result ProcessReport(string reportJson);
}

public sealed class CspReportService : ICspReportService
{
    private readonly ILogger<CspReportService> _logger;

    public CspReportService(ILogger<CspReportService> logger)
    {
        _logger = logger;
    }

    public Result ProcessReport(string reportJson)
    {
        if (string.IsNullOrWhiteSpace(reportJson))
        {
            return Result.Failure(new Error("Report is empty", 400));
        }

        CspViolationReport? report;
        try
        {
            report = JsonSerializer.Deserialize<CspViolationReport>(reportJson);
        }
        catch (JsonException)
        {
            return Result.Failure(new Error("Invalid CSP report", 400));
        }

        if (report?.CspReport != null)
        {
            _logger.LogWarning(
                "CSP Violation: {DocumentUri} blocked {ViolatedDirective} from {BlockedUri}. Original Policy: {OriginalPolicy}",
                report.CspReport.DocumentUri,
                report.CspReport.ViolatedDirective,
                report.CspReport.BlockedUri,
                report.CspReport.OriginalPolicy);
        }

        return Result.Success();
    }
}
