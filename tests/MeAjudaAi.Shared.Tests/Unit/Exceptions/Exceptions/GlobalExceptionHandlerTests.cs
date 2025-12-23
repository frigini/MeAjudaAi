using System.Text.Json;
using FluentAssertions;
using FluentValidation.Results;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using ValidationEx = MeAjudaAi.Shared.Exceptions.ValidationException;

namespace MeAjudaAi.Shared.Tests.Exceptions;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerTests()
    {
        _handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldReturn400()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Email", "Email is invalid")
        };
        var exception = new ValidationEx(failures);

        // Act
        var handled = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_ShouldReturn404()
    {
        // Arrange
        var exception = new NotFoundException("Provider", Guid.NewGuid());

        // Act
        var handled = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnauthorizedAccessException_ShouldReturn401()
    {
        // Arrange
        var exception = new UnauthorizedAccessException();

        // Act
        var handled = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task TryHandleAsync_WithUniqueConstraintException_ShouldReturn409()
    {
        // Arrange
        var innerException = new Exception("Duplicate key error");
        var exception = new UniqueConstraintException("unique_email", "Email", innerException);

        // Act
        var handled = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task TryHandleAsync_WithNotNullConstraintException_ShouldReturn400()
    {
        // Arrange
        var innerException = new Exception("Null value error");
        var exception = new NotNullConstraintException("Name", innerException, isColumnName: true);

        // Act
        var handled = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_WithForeignKeyConstraintException_ShouldReturn400()
    {
        // Arrange
        var innerException = new Exception("FK violation");
        var exception = new ForeignKeyConstraintException("fk_provider", "providers", innerException);

        // Act
        var handled = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnhandledException_ShouldReturn500()
    {
        // Arrange
        var exception = new InvalidOperationException("Unexpected error");

        // Act
        var handled = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldIncludeErrors()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Name", "Name must be at least 3 characters"),
            new("Email", "Email is invalid")
        };
        var exception = new ValidationEx(failures);

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("title").GetString().Should().Be("Validation Error");
        problemDetails.TryGetProperty("errors", out var errorsElement).Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_ShouldIncludeEntityInfo()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var exception = new NotFoundException("Provider", entityId);

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("title").GetString().Should().Be("Resource Not Found");
        problemDetails.GetProperty("detail").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TryHandleAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var exception = new InvalidOperationException("Test");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await _handler.TryHandleAsync(_httpContext, exception, cts.Token));
    }
}
