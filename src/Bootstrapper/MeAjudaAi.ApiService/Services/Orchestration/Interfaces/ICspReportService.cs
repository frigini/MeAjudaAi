using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.ApiService.Services.Orchestration.Interfaces;

public interface ICspReportService
{
    Result ProcessReport(string reportJson);
}
