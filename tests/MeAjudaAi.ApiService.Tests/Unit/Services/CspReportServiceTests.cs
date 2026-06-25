using FluentAssertions;
using MeAjudaAi.ApiService.Endpoints.Models;
using MeAjudaAi.ApiService.Services.Orchestration;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.ApiService.Tests.Unit.Services;

public class CspReportServiceTests
{
    private readonly Mock<ISerializer> _serializerMock;
    private readonly Mock<ILogger<CspReportService>> _loggerMock;
    private readonly CspReportService _service;

    public CspReportServiceTests()
    {
        _serializerMock = new Mock<ISerializer>();
        _loggerMock = new Mock<ILogger<CspReportService>>();
        _service = new CspReportService(_serializerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void ProcessReport_WithEmptyJson_ShouldReturnFailure()
    {
        var result = _service.ProcessReport("");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ProcessReport_WithWhitespaceJson_ShouldReturnFailure()
    {
        var result = _service.ProcessReport("   ");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ProcessReport_WhenDeserializerThrowsException_ShouldReturnFailure()
    {
        _serializerMock
            .Setup(s => s.Deserialize<CspViolationReport>(It.IsAny<string>()))
            .Throws(new System.Text.Json.JsonException("Deserializer failed"));

        var result = _service.ProcessReport("{ invalid json }");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Invalid CSP report");
    }

    [Fact]
    public void ProcessReport_WithValidJson_ShouldReturnSuccess()
    {
        var cspReport = new CspViolationReport
        {
            CspReport = new CspReportDetails
            {
                DocumentUri = "https://test.com",
                ViolatedDirective = "script-src",
                BlockedUri = "https://malicious.com"
            }
        };

        _serializerMock
            .Setup(s => s.Deserialize<CspViolationReport>(It.IsAny<string>()))
            .Returns(cspReport);

        var json = """
            {
                "csp-report": {
                    "document-uri": "https://test.com",
                    "violated-directive": "script-src",
                    "blocked-uri": "https://malicious.com"
                }
            }
            """;

        var result = _service.ProcessReport(json);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ProcessReport_WithNullCspReport_ShouldReturnSuccess()
    {
        _serializerMock
            .Setup(s => s.Deserialize<CspViolationReport>(It.IsAny<string>()))
            .Returns(new CspViolationReport { CspReport = null });

        var result = _service.ProcessReport("{}");

        result.IsSuccess.Should().BeTrue();
    }
}
