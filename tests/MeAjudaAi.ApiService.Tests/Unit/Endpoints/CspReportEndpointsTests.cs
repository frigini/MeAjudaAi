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
        // Invoke the private static method using reflection or just make it internal
        // But since it's used in MapPost, we can test it through a delegate if we wanted to be strictly integration
        // However, we can call it directly if we have access.
        
        // As it is private static, I'll use reflection for unit testing the logic
        var method = typeof(CspReportEndpoints).GetMethod("ReceiveCspReport", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        var resultTask = (Task<IResult>)method!.Invoke(null, new object[] { context, _loggerMock.Object })!;
        var result = await resultTask;

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
        var method = typeof(CspReportEndpoints).GetMethod("ReceiveCspReport", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        var resultTask = (Task<IResult>)method!.Invoke(null, new object[] { context, _loggerMock.Object })!;
        var result = await resultTask;

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
