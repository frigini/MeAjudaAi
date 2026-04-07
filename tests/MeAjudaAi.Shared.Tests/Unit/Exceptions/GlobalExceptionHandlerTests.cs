using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
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
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Erro de validação");
        problemDetails.Extensions.Should().ContainKey("errors");
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
        problemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);
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
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Recurso Não Encontrado");
        problemDetails.Extensions["entityName"].ToString().Should().Be("Resource");
        problemDetails.Extensions["entityId"].ToString().Should().Be("123");
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
        problemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
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
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Violação de Regra de Negócio");
        problemDetails.Extensions["ruleName"].ToString().Should().Be("MyRule");
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
        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
        problemDetails.Title.Should().Be("Erro Interno do Servidor");
    }

    [Fact]
    public async Task TryHandleAsync_WithUnprocessableEntityException_ShouldReturn422()
    {
        var context = CreateDefaultContext();
        var exception = new UnprocessableEntityException("Invalid state", "User");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails.Title.Should().Be("Entidade Não Processável");
        problemDetails.Detail.Should().Be("Invalid state");
        problemDetails.Extensions["entityName"].Should().Be("User");
    }

    [Fact]
    public async Task TryHandleAsync_WithUnprocessableEntityExceptionWithDetails_ShouldReturn422()
    {
        var context = CreateDefaultContext();
        var details = new Dictionary<string, object?> { ["extra"] = "info" };
        var exception = new UnprocessableEntityException("Invalid state", "User", details);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Extensions["details"].Should().NotBeNull();
    }

    [Fact]
    public async Task TryHandleAsync_WithUniqueConstraintException_ShouldReturn409()
    {
        var context = CreateDefaultContext();
        var exception = new UniqueConstraintException("unique_email", "email", new Exception("inner"));

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Valor Duplicado");
        problemDetails.Extensions["constraintName"].Should().Be("unique_email");
        problemDetails.Extensions["columnName"].Should().Be("email");
    }

    [Fact]
    public async Task TryHandleAsync_WithNotNullConstraintException_ShouldReturn400()
    {
        var context = CreateDefaultContext();
        var exception = new NotNullConstraintException("name", new Exception("inner"), true);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Campo Obrigatório Ausente");
        problemDetails.Extensions["columnName"].Should().Be("name");
    }

    [Fact]
    public async Task TryHandleAsync_WithForeignKeyConstraintException_ShouldReturn400()
    {
        var context = CreateDefaultContext();
        var exception = new ForeignKeyConstraintException("fk_user_role", "roles", new Exception("inner"));

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Referência Inválida");
        problemDetails.Extensions["constraintName"].Should().Be("fk_user_role");
        problemDetails.Extensions["tableName"].Should().Be("roles");
    }

    [Fact]
    public async Task TryHandleAsync_WithArgumentException_ShouldReturn400()
    {
        var context = CreateDefaultContext();
        var exception = new ArgumentException("Invalid argument", "paramName");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Extensions["parameterName"].Should().Be("paramName");
    }

    [Fact]
    public async Task TryHandleAsync_WithDomainException_ShouldReturn400()
    {
        var context = CreateDefaultContext();
        var exception = new TestDomainException("Domain rule violated");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Violação de Regra de Domínio");
        problemDetails.Detail.Should().Be("Domain rule violated");
    }

    [Fact]
    public async Task TryHandleAsync_WithWrappedValidationException_ShouldUnwrapAndHandle()
    {
        var context = CreateDefaultContext();
        var innerException = new MeAjudaAi.Shared.Exceptions.ValidationException(new List<ValidationFailure> 
        { 
            new("Email", "Invalid email") 
        });
        var exception = new System.Reflection.TargetInvocationException(innerException);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_WithWrappedBusinessRuleException_ShouldUnwrapAndHandle()
    {
        var context = CreateDefaultContext();
        // Use BusinessRuleException (concrete implementation) wrapped in InvalidOperationException
        var innerException = new BusinessRuleException(ruleName: "TestRule", message: "Wrapped rule violation");
        var outerException = new InvalidOperationException("Wrapper", innerException);

        var result = await _handler.TryHandleAsync(context, outerException, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Title.Should().Be("Violação de Regra de Negócio");
    }

    [Fact]
    public async Task TryHandleAsync_WithWrappedNotFoundException_ShouldUnwrapAndHandle()
    {
        var context = CreateDefaultContext();
        var innerException = new NotFoundException("Product", "123");
        var exception = new AggregateException(innerException);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Extensions["entityName"].Should().Be("Product");
    }

    [Fact]
    public async Task TryHandleAsync_WithDbUpdateExceptionWithUniqueViolation_ShouldReturn409()
    {
        var context = CreateDefaultContext();
        var innerException = new PostgresException("23505", "duplicate_key", "unique constraint", "users_pkey", null, null);
        var dbException = new Microsoft.EntityFrameworkCore.DbUpdateException("Update failed", innerException);

        var result = await _handler.TryHandleAsync(context, dbException, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Valor Duplicado");
    }

    [Fact]
    public async Task TryHandleAsync_WithDbUpdateExceptionWithNotNullViolation_ShouldReturn400()
    {
        var context = CreateDefaultContext();
        var innerException = new PostgresException("23502", "not_null_violation", "null value", "users", null, null);
        var dbException = new Microsoft.EntityFrameworkCore.DbUpdateException("Update failed", innerException);

        var result = await _handler.TryHandleAsync(context, dbException, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Campo Obrigatório Ausente");
    }

    [Fact]
    public async Task TryHandleAsync_WithGenericDbUpdateException_ShouldReturn400()
    {
        var context = CreateDefaultContext();
        var dbException = new Microsoft.EntityFrameworkCore.DbUpdateException("Generic DB error", new Exception("Inner"));

        var result = await _handler.TryHandleAsync(context, dbException, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Erro de Banco de Dados");
        problemDetails.Extensions.Should().ContainKey("exceptionType");
    }

    [Fact]
    public async Task TryHandleAsync_WithServerError_ShouldLogError()
    {
        var context = CreateDefaultContext();
        var exception = new Exception("Server error");

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_WithClientError_ShouldLogWarning()
    {
        var context = CreateDefaultContext();
        var exception = new ForbiddenAccessException("Client error");

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_SetsCorrectContentType()
    {
        var context = CreateDefaultContext();
        var exception = new Exception("Test");

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task TryHandleAsync_SetsInstanceToRequestPath()
    {
        var context = CreateDefaultContext();
        context.Request.Path = "/api/v1/users/123";
        var exception = new Exception("Test");

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Instance.Should().Be("/api/v1/users/123");
    }

    [Fact]
    public async Task TryHandleAsync_ValidationExceptionWithMultipleErrors_ShouldGroupByProperty()
    {
        var context = CreateDefaultContext();
        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid email format"),
            new("Email", "Email already exists"),
            new("Name", "Name is required")
        };
        var exception = new MeAjudaAi.Shared.Exceptions.ValidationException(failures);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        var problemDetails = await ReadProblemDetailsAsync(context);
        
        var errors = problemDetails.Extensions["errors"] as IDictionary<string, string[]>;
        errors.Should().NotBeNull();
        errors.Should().ContainKey("Email");
        errors["Email"].Should().HaveCount(2);
        errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task TryHandleAsync_UniqueConstraintWithNullColumnName_ShouldHandleGracefully()
    {
        var context = CreateDefaultContext();
        var exception = new UniqueConstraintException("constraint", null, new Exception("inner"));

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Detail.Should().Contain("este campo");
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundExceptionWithNullId_ShouldHandleGracefully()
    {
        var context = CreateDefaultContext();
        var exception = new NotFoundException("Resource", null!);

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Extensions.Should().NotContainKey("entityId");
    }

    [Fact]
    public async Task TryHandleAsync_BusinessRuleExceptionWithoutRuleName_ShouldHandleGracefully()
    {
        var context = CreateDefaultContext();
        var exception = new BusinessRuleException(string.Empty, "Business rule broken");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        var problemDetails = await ReadProblemDetailsAsync(context);
        problemDetails.Extensions.Should().NotContainKey("ruleName");
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
    private class TestBadRequestException(string message) : BadRequestException(message) { }
}