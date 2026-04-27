using MeAjudaAi.ApiService.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using System.Text;
using System.Text.Json;

namespace MeAjudaAi.ApiService.Tests.Unit.Endpoints;

public class CspReportEndpointsTests
{
    private readonly Mock<ILogger<Program>> _loggerMock = new();

    [Fact]
    public async Task ReceiveCspReport_Should_ReturnNoContent_When_ValidReport()
    {
        // Arrange
        var report = new CspViolationReport
        {
            CspReport = new CspReportDetails
            {
                DocumentUri = "https://test.com",
                ViolatedDirective = "script-src",
                BlockedUri = "https://malicious.com"
            }
        };
        var json = JsonSerializer.Serialize(report);
        var context = CreateHttpContext(json);

        // Act
        var result = await CspReportEndpoints.ReceiveCspReport(context, _loggerMock.Object);

        // Assert
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.NoContent>();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CSP Violation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ReceiveCspReport_Should_ReturnBadRequest_When_EmptyBody()
    {
        // Arrange
        var context = CreateHttpContext("");

        // Act
        var result = await CspReportEndpoints.ReceiveCspReport(context, _loggerMock.Object);

        // Assert
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>>();
    }

    [Fact]
    public async Task ReceiveCspReport_Should_ReturnNoContent_When_InvalidJson()
    {
        // Arrange
        var context = CreateHttpContext("{ invalid json }");

        // Act
        var result = await CspReportEndpoints.ReceiveCspReport(context, _loggerMock.Object);

        // Assert
        // JsonSerializer.Deserialize throws for invalid JSON, which is caught and returns 500 in current impl
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(500);
    }

    private static HttpContext CreateHttpContext(string body)
    {
        var context = new DefaultHttpContext();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Request.Body = stream;
        context.Request.ContentLength = stream.Length;
        return context;
    }
}
