using FluentAssertions;
using MeAjudaAi.ApiService.Services.Orchestration;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.ApiService.Tests.Unit.Services;

public class CspReportServiceTests
{
    private readonly Mock<ILogger<CspReportService>> _loggerMock;

    public CspReportServiceTests()
    {
        _loggerMock = new Mock<ILogger<CspReportService>>();
    }

    [Fact]
    public void ProcessReport_WithEmptyJson_ShouldReturnFailure()
    {
        var service = new CspReportService(_loggerMock.Object);

        var result = service.ProcessReport("");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ProcessReport_WithWhitespaceJson_ShouldReturnFailure()
    {
        var service = new CspReportService(_loggerMock.Object);

        var result = service.ProcessReport("   ");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ProcessReport_WithInvalidJson_ShouldReturnFailure()
    {
        var service = new CspReportService(_loggerMock.Object);

        var result = service.ProcessReport("{ invalid json }");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ProcessReport_WithValidJson_ShouldReturnSuccess()
    {
        var service = new CspReportService(_loggerMock.Object);
        var json = """
            {
                "csp-report": {
                    "document-uri": "https://test.com",
                    "violated-directive": "script-src",
                    "blocked-uri": "https://malicious.com"
                }
            }
            """;

        var result = service.ProcessReport(json);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ProcessReport_WithNullCspReport_ShouldReturnSuccess()
    {
        var service = new CspReportService(_loggerMock.Object);
        var json = "{}";

        var result = service.ProcessReport(json);

        result.IsSuccess.Should().BeTrue();
    }
}
