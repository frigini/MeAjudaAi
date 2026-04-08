using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging.Handlers;

public class TestMessage { public string Content { get; init; } = ""; }

[Trait("Category", "Unit")]
public class MessageRetryMiddlewareTests
{
    private readonly Mock<IDeadLetterService> _deadLetterServiceMock;
    private readonly Mock<ILogger<MessageRetryMiddleware<TestMessage>>> _loggerMock;
    private readonly MessageRetryMiddleware<TestMessage> _middleware;

    public MessageRetryMiddlewareTests()
    {
        _deadLetterServiceMock = new Mock<IDeadLetterService>();
        _loggerMock = new Mock<ILogger<MessageRetryMiddleware<TestMessage>>>();
        _middleware = new MessageRetryMiddleware<TestMessage>(
            _deadLetterServiceMock.Object,
            _loggerMock.Object,
            "TestHandler",
            "test-queue");
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WhenSuccessful_ShouldReturnTrueAndNotCallDLQ()
    {
        // Arrange
        var message = new TestMessage { Content = "Hello" };
        var handlerCalled = 0;
        Func<TestMessage, CancellationToken, Task> handler = (m, ct) => { handlerCalled++; return Task.CompletedTask; };

        // Act
        var result = await _middleware.ExecuteWithRetryAsync(message, handler);

        // Assert
        result.Should().BeTrue();
        handlerCalled.Should().Be(1);
        _deadLetterServiceMock.Verify(d => d.SendToDeadLetterAsync(It.IsAny<TestMessage>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WhenFailsThenSucceeds_ShouldRetryAndReturnTrue()
    {
        // Arrange
        var message = new TestMessage { Content = "Retry" };
        var handlerCalled = 0;
        Func<TestMessage, CancellationToken, Task> handler = (m, ct) => 
        { 
            handlerCalled++; 
            if (handlerCalled == 1) throw new Exception("Transient error");
            return Task.CompletedTask; 
        };

        _deadLetterServiceMock.Setup(d => d.ShouldRetry(It.IsAny<Exception>(), 1)).Returns(true);
        _deadLetterServiceMock.Setup(d => d.CalculateRetryDelay(1)).Returns(TimeSpan.Zero);

        // Act
        var result = await _middleware.ExecuteWithRetryAsync(message, handler);

        // Assert
        result.Should().BeTrue();
        handlerCalled.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WhenFailsPermanently_ShouldCallDLQAndReturnFalse()
    {
        // Arrange
        var message = new TestMessage { Content = "Permanent Failure" };
        var handlerCalled = 0;
        Func<TestMessage, CancellationToken, Task> handler = (m, ct) => 
        { 
            handlerCalled++; 
            throw new Exception("Fatal error");
        };

        _deadLetterServiceMock.Setup(d => d.ShouldRetry(It.IsAny<Exception>(), It.IsAny<int>())).Returns(false);

        // Act
        var result = await _middleware.ExecuteWithRetryAsync(message, handler);

        // Assert
        result.Should().BeFalse();
        _deadLetterServiceMock.Verify(d => d.SendToDeadLetterAsync(
            message, It.IsAny<Exception>(), "TestHandler", "test-queue", 1, It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
