using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Moq;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Database.Exceptions;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using System.IO;
using Npgsql;
using FluentValidation;
using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

[Trait("Category", "Unit")]
public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly Mock<IHostEnvironment> _envMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _envMock = new Mock<IHostEnvironment>();
        
        // Padrão para Development para testes existentes
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        
        _handler = new GlobalExceptionHandler(_loggerMock.Object, _envMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldReturnBadRequest()
    {
        var context = CreateDefaultContext();
        var failures = new List<ValidationFailure>
        {
            new("PropertyName", "ErrorMessage")
        };
        var exception = new MeAjudaAi.Shared.Exceptions.ValidationException(failures);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var body = await ReadProblemDetailsAsync(context);
        body.Title.Should().Be("Erro de validação");
        body.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public async Task TryHandleAsync_WithUniqueConstraintException_ShouldReturnConflict()
    {
        var context = CreateDefaultContext();
        var exception = new UniqueConstraintException("IX_Test", "Email", null!);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        var body = await ReadProblemDetailsAsync(context);
        body.Title.Should().Be("Valor Duplicado");
        body.Extensions.Should().ContainKey("columnName");
    }

    [Fact]
    public async Task TryHandleAsync_WithNotNullConstraintException_ShouldReturnBadRequest()
    {
        var context = CreateDefaultContext();
        var exception = new NotNullConstraintException("Name", null!, true);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var body = await ReadProblemDetailsAsync(context);
        body.Title.Should().Be("Campo Obrigatório Ausente");
    }

    [Fact]
    public async Task TryHandleAsync_WithForeignKeyConstraintException_ShouldReturnBadRequest()
    {
        var context = CreateDefaultContext();
        var exception = new ForeignKeyConstraintException("FK_Test", "Users", null!);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var body = await ReadProblemDetailsAsync(context);
        body.Title.Should().Be("Referência Inválida");
    }

    [Fact]
    public async Task TryHandleAsync_WithAggregateException_ShouldUnwrapAndHandleInner()
    {
        var context = CreateDefaultContext();
        var inner = new MeAjudaAi.Shared.Exceptions.NotFoundException("User", "123");
        var exception = new AggregateException(inner);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnauthorizedAccessException_ShouldReturnUnauthorized()
    {
        var context = CreateDefaultContext();
        var exception = new UnauthorizedAccessException();

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Title.Should().Be("Não Autorizado");
    }

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_ShouldReturnNotFound()
    {
        var context = CreateDefaultContext();
        var exception = new NotFoundException("Resource", "123");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Title.Should().Be("Recurso Não Encontrado");
        problemDetails.Extensions["entityName"]?.ToString().Should().Be("Resource");
        problemDetails.Extensions["entityId"]?.ToString().Should().Be("123");
    }

    [Fact]
    public async Task TryHandleAsync_WithForbiddenAccessException_ShouldReturnForbidden()
    {
        var context = CreateDefaultContext();
        var exception = new ForbiddenAccessException("Access denied");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Title.Should().Be("Acesso Negado");
        problemDetails.Detail.Should().Be("Access denied");
    }

    [Fact]
    public async Task TryHandleAsync_WithBusinessRuleException_ShouldReturnBadRequest()
    {
        var context = CreateDefaultContext();
        var exception = new BusinessRuleException("MyRule", "Rule broken");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Title.Should().Be("Violação de Regra de Negócio");
        problemDetails.Extensions["ruleName"]?.ToString().Should().Be("MyRule");
    }

    [Fact]
    public async Task TryHandleAsync_WithUnprocessableEntityException_ShouldReturn422()
    {
        var context = CreateDefaultContext();
        var exception = new UnprocessableEntityException("Invalid state", "User");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        var body = await ReadProblemDetailsAsync(context);
        body.Title.Should().Be("Entidade Não Processável");
    }

    [Fact]
    public async Task TryHandleAsync_WithGenericException_ShouldReturnInternalServerError()
    {
        var context = CreateDefaultContext();
        var exception = new Exception("Server went boom");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Title.Should().Be("Erro Interno do Servidor");
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_ShouldHideDiagnosticDetails()
    {
        // Arrange
        var context = CreateDefaultContext();
        var exception = new Exception("Sensitive internal error details");
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Detail.Should().Be("Ocorreu um erro inesperado");
        
        var serializedPayload = System.Text.Json.JsonSerializer.Serialize(problemDetails);
        serializedPayload.Should().NotContain("Sensitive internal error details");
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

    private class TestDomainException(string message) : DomainException(message) { }
}
