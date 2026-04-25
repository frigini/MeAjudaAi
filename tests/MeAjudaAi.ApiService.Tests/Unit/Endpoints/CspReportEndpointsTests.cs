using System.Text;
using System.Text.Json;
using MeAjudaAi.ApiService.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Endpoints;

[Trait("Category", "Unit")]
public class CspReportEndpointsTests
{
    private readonly Mock<ILogger<Program>> _loggerMock = new();

    [Fact]
    public async Task ReceiveCspReport_WithValidReport_ShouldReturnNoContent()
    {
        // Arrange
        var report = new CspViolationReport
        {
            CspReport = new CspReportDetails
            {
                DocumentUri = "https://example.com",
                ViolatedDirective = "script-src",
                BlockedUri = "https://evil.com",
                OriginalPolicy = "default-src 'self'"
            }
        };
        var json = JsonSerializer.Serialize(report);
        var context = CreateContextWithBody(json);

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
    public async Task ReceiveCspReport_WithEmptyBody_ShouldReturnBadRequest()
    {
        // Arrange
        var context = CreateContextWithBody("");

        // Act
        var result = await CspReportEndpoints.ReceiveCspReport(context, _loggerMock.Object);

        // Assert
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>>();
    }

    private static HttpContext CreateContextWithBody(string body)
    {
        var context = new DefaultHttpContext();
        var bytes = Encoding.UTF8.GetBytes(body);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        return context;
    }
}
       // Arrange
        var json = "{\"csp-report\": null}";
        var context = CreateContextWithBody(json);

        // Act
        var result = await CspReportEndpoints.ReceiveCspReport(context, _loggerMock.Object);

        // Assert
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.NoContent>();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task ReceiveCspReport_WithMalformedJson_ShouldReturnInternalServerError()
    {
        // Arrange
        var json = "{ invalid json }";
        var context = CreateContextWithBody(json);

        // Act
        var result = await CspReportEndpoints.ReceiveCspReport(context, _loggerMock.Object);

        // Assert
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>();
        ((Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult)result).StatusCode.Should().Be(500);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing CSP report")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ReceiveCspReport_WithEmptyBody_ShouldReturnBadRequest()
    {
        // Arrange
        var context = CreateContextWithBody("");

        // Act
        var result = await CspReportEndpoints.ReceiveCspReport(context, _loggerMock.Object);

        // Assert
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>>();
    }

    private static HttpContext CreateContextWithBody(string body)
    {
        var context = new DefaultHttpContext();
        var bytes = Encoding.UTF8.GetBytes(body);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        return context;
    }
}
