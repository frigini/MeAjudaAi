using FluentAssertions;
using MeAjudaAi.Shared.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace MeAjudaAi.Shared.Tests.Logging;

public class LoggingContextMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly LoggingContextMiddleware _middleware;
    private readonly DefaultHttpContext _context;

    public LoggingContextMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new LoggingContextMiddleware(
            _nextMock.Object,
            NullLogger<LoggingContextMiddleware>.Instance);
        _context = new DefaultHttpContext();
        _context.Request.Method = "GET";
        _context.Request.Path = "/api/test";
    }

    [Fact]
    public async Task InvokeAsync_ShouldGenerateCorrelationId_WhenNotProvided()
    {
        // Arrange
        _nextMock.Setup(next => next(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
        _context.Response.Headers["X-Correlation-ID"].ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseProvidedCorrelationId_WhenPresent()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _context.Request.Headers["X-Correlation-ID"] = correlationId;
        _nextMock.Setup(next => next(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(correlationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextDelegate()
    {
        // Arrange
        _nextMock.Setup(next => next(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _nextMock.Verify(next => next(_context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldRethrowException_WhenNextDelegateFails()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test error");
        _nextMock.Setup(next => next(_context)).ThrowsAsync(expectedException);

        // Act
        var act = async () => await _middleware.InvokeAsync(_context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test error");
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddCorrelationIdToResponse_EvenOnException()
    {
        // Arrange
        _nextMock.Setup(next => next(_context)).ThrowsAsync(new Exception("Error"));

        // Act
        try
        {
            await _middleware.InvokeAsync(_context);
        }
        catch
        {
            // Expected
        }

        // Assert
        _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
    }
}

public class LoggingExtensionsTests
{
    [Fact]
    public void UseLoggingContext_ShouldReturnApplicationBuilder()
    {
        // Arrange
        var app = new ApplicationBuilder(new Mock<IServiceProvider>().Object);

        // Act
        var result = app.UseLoggingContext();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void PushUserContext_WithUserId_ShouldReturnDisposable()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        var disposable = logger.PushUserContext("user123");

        // Assert
        disposable.Should().NotBeNull();
        disposable.Should().BeAssignableTo<IDisposable>();
        disposable.Dispose(); // Should not throw
    }

    [Fact]
    public void PushUserContext_WithUserIdAndUsername_ShouldReturnDisposable()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        var disposable = logger.PushUserContext("user123", "john.doe");

        // Assert
        disposable.Should().NotBeNull();
        disposable.Dispose(); // Should not throw
    }

    [Fact]
    public void PushUserContext_WithNullUserId_ShouldReturnDisposable()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        var disposable = logger.PushUserContext(null);

        // Assert
        disposable.Should().NotBeNull();
        disposable.Dispose(); // Should not throw
    }

    [Fact]
    public void PushOperationContext_WithOperation_ShouldReturnDisposable()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        var disposable = logger.PushOperationContext("CreateProvider");

        // Assert
        disposable.Should().NotBeNull();
        disposable.Dispose(); // Should not throw
    }

    [Fact]
    public void PushOperationContext_WithOperationAndParameters_ShouldReturnDisposable()
    {
        // Arrange
        var logger = NullLogger.Instance;
        var parameters = new { Name = "Test", Type = "Individual" };

        // Act
        var disposable = logger.PushOperationContext("CreateProvider", parameters);

        // Assert
        disposable.Should().NotBeNull();
        disposable.Dispose(); // Should not throw
    }

    [Fact]
    public void PushOperationContext_WithNullParameters_ShouldReturnDisposable()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        var disposable = logger.PushOperationContext("CreateProvider", null);

        // Assert
        disposable.Should().NotBeNull();
        disposable.Dispose(); // Should not throw
    }

    [Fact]
    public void PushUserContext_MultipleCalls_ShouldBeIndependent()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        var disposable1 = logger.PushUserContext("user1");
        var disposable2 = logger.PushUserContext("user2");

        // Assert
        disposable1.Should().NotBeSameAs(disposable2);
        disposable1.Dispose();
        disposable2.Dispose();
    }
}
