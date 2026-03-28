using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MeAjudaAi.Shared.Exceptions;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using System.IO;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

[Trait("Category", "Unit")]
public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldReturnBadRequest()
    {
        // Arrange
        var context = CreateDefaultContext();
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("PropertyName", "ErrorMessage")
        };
        var exception = new MeAjudaAi.Shared.Exceptions.ValidationException(failures);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Erro de validação");
        problemDetails.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public async Task TryHandleAsync_WithUnauthorizedAccessException_ShouldReturnUnauthorized()
    {
        // Arrange
        var context = CreateDefaultContext();
        var exception = new UnauthorizedAccessException();

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails.Title.Should().Be("Não Autorizado");
    }

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_ShouldReturnNotFound()
    {
        // Arrange
        var context = CreateDefaultContext();
        var exception = new NotFoundException("Resource", "123");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Recurso Não Encontrado");
        problemDetails.Extensions["entityName"].ToString().Should().Be("Resource");
        problemDetails.Extensions["entityId"].ToString().Should().Be("123");
    }

    [Fact]
    public async Task TryHandleAsync_WithForbiddenAccessException_ShouldReturnForbidden()
    {
        // Arrange
        var context = CreateDefaultContext();
        var exception = new ForbiddenAccessException("Access denied");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemDetails.Title.Should().Be("Acesso Negado");
        problemDetails.Detail.Should().Be("Access denied");
    }

    [Fact]
    public async Task TryHandleAsync_WithBusinessRuleException_ShouldReturnBadRequest()
    {
        // Arrange
        var context = CreateDefaultContext();
        var exception = new BusinessRuleException("Rule broken", "MyRule");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Violação de Regra de Negócio");
        problemDetails.Extensions["ruleName"].ToString().Should().Be("MyRule");
    }

    [Fact]
    public async Task TryHandleAsync_WithGenericException_ShouldReturnInternalServerError()
    {
        // Arrange
        var context = CreateDefaultContext();
        var exception = new Exception("Server went boom");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
        problemDetails.Title.Should().Be("Erro Interno do Servidor");
    }

    private DefaultHttpContext CreateDefaultContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/test";
        context.TraceIdentifier = "trace-123";
        return context;
    }

    private async Task<ProblemDetails> ReadProblemDetailsAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
