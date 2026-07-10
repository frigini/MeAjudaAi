using MeAjudaAi.ApiService.Endpoints.Models;
using MeAjudaAi.ApiService.Services.Orchestration;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.ApiService.Tests.Unit.Services.Orchestration;

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
        // Arrange
        var json = "";

        // Act
        var result = _service.ProcessReport(json);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ProcessReport_WithWhitespaceJson_ShouldReturnFailure()
    {
        // Arrange
        var json = "   ";

        // Act
        var result = _service.ProcessReport(json);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ProcessReport_WhenDeserializerThrowsException_ShouldReturnFailure()
    {
        // Arrange
        _serializerMock
            .Setup(s => s.Deserialize<CspViolationReport>(It.IsAny<string>()))
            .Throws(new System.Text.Json.JsonException("Deserializer failed"));

        // Act
        var result = _service.ProcessReport("{ invalid json }");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Invalid CSP report");
    }

    [Fact]
    public void ProcessReport_WithValidJson_ShouldReturnSuccess()
    {
        // Arrange
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

        // Act
        var result = _service.ProcessReport(json);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ProcessReport_WithNullCspReport_ShouldReturnSuccess()
    {
        // Arrange
        _serializerMock
            .Setup(s => s.Deserialize<CspViolationReport>(It.IsAny<string>()))
            .Returns(new CspViolationReport { CspReport = null });

        // Act
        var result = _service.ProcessReport("{}");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
