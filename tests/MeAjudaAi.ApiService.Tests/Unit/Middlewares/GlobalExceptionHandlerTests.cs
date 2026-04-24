using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MeAjudaAi.ApiService.Tests.Unit.Middlewares;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
    private readonly Mock<IHostEnvironment> _mockEnv;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        _mockEnv = new Mock<IHostEnvironment>();
        
        _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        
        _handler = new GlobalExceptionHandler(_mockLogger.Object, _mockEnv.Object);
    }

    [Fact]
    public async Task TryHandleAsync_WithArgumentException_ShouldReturnBadRequest()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ArgumentException("Invalid argument");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_ShouldReturnNotFound()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new NotFoundException("Booking", Guid.NewGuid());

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task TryHandleAsync_WithForbiddenAccessException_ShouldReturnForbidden()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ForbiddenAccessException("Access denied");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task TryHandleAsync_WithBusinessRuleException_ShouldReturnBadRequest()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new BusinessRuleException("BookingRule", "Business rule violation");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_WithGenericException_ShouldReturnInternalServerError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Something went wrong");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_InDevelopment_ShouldShowExceptionDetails()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "trace-dev-123";
        var exception = new Exception("Development error details");
        _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status500InternalServerError);
        problemDetails.Detail.Should().Contain("Development error details");
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"].ToString().Should().Be("trace-dev-123");
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_ShouldHideExceptionDetails()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "trace-abc";
        var exception = new InvalidOperationException("Dados sensíveis do sistema interno");
        _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status500InternalServerError);
        problemDetails.Detail.Should().NotContain("Dados sensíveis");
        problemDetails.Detail.Should().Contain("Ocorreu um erro inesperado");
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"].ToString().Should().Be("trace-abc");
    }
}