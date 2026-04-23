using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
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
        
        // Configura ambiente de desenvolvimento por padrão para os testes existentes
        _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        
        _handler = new GlobalExceptionHandler(_mockLogger.Object, _mockEnv.Object);
    }

    [Fact]
    public async Task TryHandleAsync_WithArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ArgumentException("Invalid argument");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_WithArgumentNullException_ShouldReturnBadRequest()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ArgumentNullException("parameter");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnauthorizedAccessException_ShouldReturnUnauthorized()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new UnauthorizedAccessException("Access denied");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task TryHandleAsync_WithGenericException_ShouldReturnInternalServerError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldLogError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Server error occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetCorrectContentType()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnErrorResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;
        var exception = new ArgumentException("Invalid argument");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        responseStream.Position = 0;
#pragma warning disable CA2000 // StreamReader será disposto com o responseStream
        var responseContent = await new StreamReader(responseStream).ReadToEndAsync();
#pragma warning restore CA2000
        responseContent.Should().NotBeEmpty();

        var errorResponse = JsonSerializer.Deserialize<object>(responseContent);
        errorResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_ShouldHideExceptionDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "trace-abc";
        var exception = new Exception("Dados sensíveis do sistema interno");
        _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().NotContain("Dados sensíveis");
        body.Should().Contain("Ocorreu um erro inesperado");
        body.Should().Contain("trace-abc");
    }
}
