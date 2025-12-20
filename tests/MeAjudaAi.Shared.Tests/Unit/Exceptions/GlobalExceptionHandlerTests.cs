using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentValidation.Results;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using ValidationException = MeAjudaAi.Shared.Exceptions.ValidationException;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

/// <summary>
/// Testes unitários para GlobalExceptionHandler
/// Cobertura: TryHandleAsync, ProcessDbUpdateException, GetProblemTypeUri
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "GlobalExceptionHandler")]
public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _handler;
    private readonly DefaultHttpContext _httpContext;

    // Classe concreta de DomainException para testes
    private sealed class TestDomainException : DomainException
    {
        public TestDomainException(string message) : base(message) { }
    }

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
        _httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test" },
            TraceIdentifier = "test-trace-id"
        };
        _httpContext.Response.Body = new MemoryStream();
    }

    #region ValidationException Tests

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldReturn400WithValidationErrors()
    {
        // Arrange
        var validationErrors = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Email", "Email is invalid"),
            new("Email", "Email already exists")
        };
        var exception = new ValidationException(validationErrors);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _httpContext.Response.ContentType.Should().Contain("json");

        var problemDetails = await DeserializeProblemDetails();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(400);
        problemDetails.Title.Should().Be("Validation Error");
        problemDetails.Detail.Should().Be("One or more validation errors occurred");
        problemDetails.Instance.Should().Be("/api/test");

        var errors = problemDetails.Extensions["errors"] as JsonElement?;
        errors.Should().NotBeNull();
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldGroupErrorsByProperty()
    {
        // Arrange
        var validationErrors = new List<ValidationFailure>
        {
            new("Email", "Email is required"),
            new("Email", "Email is invalid"),
            new("Name", "Name is required")
        };
        var exception = new ValidationException(validationErrors);

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails();
        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey("errors");
    }

    #endregion

    #region UniqueConstraintException Tests

    [Fact]
    public async Task TryHandleAsync_WithUniqueConstraintException_ShouldReturn409()
    {
        // Arrange
        var innerException = new Exception("Inner exception");
        var exception = new UniqueConstraintException("uk_users_email", "email", innerException);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var problemDetails = await DeserializeProblemDetails();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(409);
        problemDetails.Title.Should().Be("Duplicate Value");
        problemDetails.Detail.Should().Contain("email");
        problemDetails.Extensions.Should().ContainKey("constraintName");
        problemDetails.Extensions.Should().ContainKey("columnName");
    }

    [Fact]
    public async Task TryHandleAsync_WithUniqueConstraintException_NoColumnName_ShouldUseDefaultMessage()
    {
        // Arrange
        var innerException = new Exception("Inner exception");
        var exception = new UniqueConstraintException("uk_constraint", columnName: null!, innerException);

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Detail.Should().Contain("this field");
    }

    #endregion

    #region NotNullConstraintException Tests

    [Fact]
    public async Task TryHandleAsync_WithNotNullConstraintException_ShouldReturn400()
    {
        // Arrange
        var innerException = new Exception("Inner exception");
        var exception = new NotNullConstraintException("username", innerException, isColumnName: true);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = await DeserializeProblemDetails();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Required Field Missing");
        problemDetails.Detail.Should().Contain("username");
        problemDetails.Extensions["columnName"].Should().NotBeNull();
    }

    #endregion

    #region ForeignKeyConstraintException Tests

    [Fact]
    public async Task TryHandleAsync_WithForeignKeyConstraintException_ShouldReturn400()
    {
        // Arrange
        var exception = new ForeignKeyConstraintException("fk_users_role_id", "users", innerException: null!);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Title.Should().Be("Invalid Reference");
        problemDetails.Detail.Should().Be("The referenced record does not exist");
        problemDetails.Extensions.Should().ContainKey("constraintName");
        problemDetails.Extensions.Should().ContainKey("tableName");
    }

    #endregion

    #region NotFoundException Tests

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_ShouldReturn404()
    {
        // Arrange
        var exception = new NotFoundException("User", "123");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Status.Should().Be(404);
        problemDetails.Title.Should().Be("Resource Not Found");
        problemDetails.Extensions["entityName"].Should().NotBeNull();
        problemDetails.Extensions["entityId"].Should().NotBeNull();
    }

    #endregion

    #region UnauthorizedAccessException Tests

    [Fact]
    public async Task TryHandleAsync_WithUnauthorizedAccessException_ShouldReturn401()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Invalid token");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Status.Should().Be(401);
        problemDetails.Title.Should().Be("Unauthorized");
        problemDetails.Detail.Should().Be("Authentication is required to access this resource");
    }

    #endregion

    #region ForbiddenAccessException Tests

    [Fact]
    public async Task TryHandleAsync_WithForbiddenAccessException_ShouldReturn403()
    {
        // Arrange
        var exception = new ForbiddenAccessException("Insufficient permissions", innerException: null!);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Status.Should().Be(403);
        problemDetails.Title.Should().Be("Forbidden");
        problemDetails.Detail.Should().Be("Insufficient permissions");
    }

    #endregion

    #region BusinessRuleException Tests

    [Fact]
    public async Task TryHandleAsync_WithBusinessRuleException_ShouldReturn400WithRuleName()
    {
        // Arrange
        var exception = new BusinessRuleException("AgeLimit", "User must be at least 18 years old");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Title.Should().Be("Business Rule Violation");
        problemDetails.Detail.Should().Be("User must be at least 18 years old");
        problemDetails.Extensions["ruleName"].Should().NotBeNull();
    }

    #endregion

    #region DomainException Tests

    [Fact]
    public async Task TryHandleAsync_WithDomainException_ShouldReturn400()
    {
        // Arrange
        var exception = new TestDomainException("Invalid email format");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Title.Should().Be("Domain Rule Violation");
        problemDetails.Detail.Should().Be("Invalid email format");
    }

    #endregion

    #region DbUpdateException Tests

    [Fact]
    public async Task TryHandleAsync_WithDbUpdateException_ShouldProcessAndReturnAppropriateStatus()
    {
        // Arrange - Cria um DbUpdateException sem exceção interna
        var exception = new DbUpdateException("Database update failed");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Title.Should().Be("Database Error");
        problemDetails.Detail.Should().Be("A database error occurred while processing your request");
    }

    #endregion

    #region Generic Exception Tests

    [Fact]
    public async Task TryHandleAsync_WithGenericException_ShouldReturn500()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Status.Should().Be(500);
        problemDetails.Title.Should().Be("Internal Server Error");
        problemDetails.Detail.Should().Be("An unexpected error occurred while processing your request");
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"]!.ToString().Should().Be("test-trace-id");
    }

    [Fact]
    public async Task TryHandleAsync_WithArgumentException_ShouldReturn400()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task TryHandleAsync_With500Error_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Server error");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_With400Error_ShouldLogWarning()
    {
        // Arrange
        var exception = new TestDomainException("Domain error");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_With404Error_ShouldLogWarning()
    {
        // Arrange
        var exception = new NotFoundException("User", "123");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    #region ProblemDetails URI Tests

    [Fact]
    public async Task TryHandleAsync_ShouldSetCorrectProblemTypeUri_For400()
    {
        // Arrange
        var exception = new TestDomainException("Test");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetCorrectProblemTypeUri_For401()
    {
        // Arrange
        var exception = new UnauthorizedAccessException();

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Type.Should().Be("https://tools.ietf.org/html/rfc7235#section-3.1");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetCorrectProblemTypeUri_For403()
    {
        // Arrange
        var exception = new ForbiddenAccessException("Forbidden", innerException: null!);

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.3");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetCorrectProblemTypeUri_For404()
    {
        // Arrange
        var exception = new NotFoundException("User", "123");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetCorrectProblemTypeUri_For409()
    {
        // Arrange
        var innerException = new Exception("Inner exception");
        var exception = new UniqueConstraintException("uk_test", "test", innerException);

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.8");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetCorrectProblemTypeUri_For500()
    {
        // Arrange
        var exception = new Exception("Server error");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails();
        problemDetails!.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task TryHandleAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var exception = new TestDomainException("Test");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        // TaskCanceledException é uma subclasse de OperationCanceledException
        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await _handler.TryHandleAsync(_httpContext, exception, cts.Token));
        ex.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private async Task<ProblemDetails?> DeserializeProblemDetails()
    {
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    #endregion
}
