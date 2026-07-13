using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.DeadLetter.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging.NoOp;

[Trait("Category", "Unit")]
[Trait("Layer", "Shared")]
[Trait("Component", "NoOpDeadLetterService")]
public class NoOpDeadLetterServiceTests
{
    private readonly Mock<ILogger<NoOpDeadLetterService>> _loggerMock = new();
    private readonly NoOpDeadLetterService _sut;

    public NoOpDeadLetterServiceTests()
    {
        _sut = new NoOpDeadLetterService(_loggerMock.Object);
    }

    [Theory]
    [InlineData(typeof(ArgumentException), 1, false, "Permanent exception should not retry")]
    [InlineData(typeof(ArgumentNullException), 1, false, "Permanent exception should not retry")]
    [InlineData(typeof(TimeoutException), 1, true, "Transient exception should retry on first attempt")]
    [InlineData(typeof(TimeoutException), 5, false, "Transient exception should not retry after max attempts")]
    [InlineData(typeof(HttpRequestException), 1, true, "HTTP exception should retry")]
    [InlineData(typeof(HttpRequestException), 3, true, "HTTP exception should retry")]
    [InlineData(typeof(HttpRequestException), 4, false, "HTTP exception should not retry after max attempts")]
    [InlineData(typeof(OutOfMemoryException), 1, false, "Critical exception should not retry")]
    public void ShouldRetry_WithDifferentExceptionTypes_ReturnsExpectedResult(
        Type exceptionType, int attemptCount, bool expectedShouldRetry, string reason)
    {
        // Arrange
        var exception = CreateException(exceptionType);

        // Act
        var shouldRetry = _sut.ShouldRetry(exception, attemptCount);

        // Assert
        shouldRetry.Should().Be(expectedShouldRetry, reason);
    }

    [Fact]
    public void CalculateRetryDelay_WithLowAttemptCount_ReturnsExponentialBackoff()
    {
        // Act
        var delay1 = _sut.CalculateRetryDelay(1);
        var delay2 = _sut.CalculateRetryDelay(2);
        var delay3 = _sut.CalculateRetryDelay(3);

        // Assert
        delay1.TotalSeconds.Should().Be(2);   // 2^(1-1) * 2 = 2
        delay2.TotalSeconds.Should().Be(4);   // 2^(2-1) * 2 = 4
        delay3.TotalSeconds.Should().Be(8);   // 2^(3-1) * 2 = 8
    }

    [Fact]
    public void CalculateRetryDelay_WithHighAttemptCount_DoesNotExceedMaxDelay()
    {
        // Arrange
        const int highAttemptCount = 10;

        // Act
        var delay = _sut.CalculateRetryDelay(highAttemptCount);

        // Assert
        delay.TotalSeconds.Should().Be(300); // Max 5 minutos
    }

    [Fact]
    public async Task SendToDeadLetterAsync_WithValidMessage_CompletesSuccessfully()
    {
        // Arrange
        var message = new TestMessage { Id = "test-123", Content = "Test content" };
        var exception = new InvalidOperationException("Test exception");
        const string handlerType = "TestHandler";
        const string sourceQueue = "test-queue";
        const int attemptCount = 3;

        // Act
        var act = () => _sut.SendToDeadLetterAsync(
            message, exception, handlerType, sourceQueue, attemptCount);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ListDeadLetterMessagesAsync_WithValidQueue_ReturnsEmptyList()
    {
        // Arrange
        const string queueName = "dlq.test-queue";

        // Act
        var messages = await _sut.ListDeadLetterMessagesAsync(queueName);

        // Assert
        messages.Should().NotBeNull();
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task ReprocessDeadLetterMessageAsync_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        const string queueName = "dlq.test-queue";
        const string messageId = "test-message-123";

        // Act
        var act = () => _sut.ReprocessDeadLetterMessageAsync(queueName, messageId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PurgeDeadLetterMessageAsync_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        const string queueName = "dlq.test-queue";
        const string messageId = "test-message-123";

        // Act
        var act = () => _sut.PurgeDeadLetterMessageAsync(queueName, messageId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetDeadLetterStatisticsAsync_ReturnsEmptyStatistics()
    {
        // Act
        var statistics = await _sut.GetDeadLetterStatisticsAsync();

        // Assert
        statistics.Should().NotBeNull();
        statistics.TotalDeadLetterMessages.Should().Be(0);
        statistics.MessagesByQueue.Should().BeEmpty();
        statistics.MessagesByExceptionType.Should().BeEmpty();
    }

    private static Exception CreateException(Type exceptionType)
    {
        return exceptionType.Name switch
        {
            nameof(ArgumentException) => new ArgumentException("Test argument exception"),
            nameof(ArgumentNullException) => new ArgumentNullException("testParam", "Test null argument"),
            nameof(TimeoutException) => new TimeoutException("Test timeout"),
            nameof(HttpRequestException) => new HttpRequestException("Test HTTP exception"),
            nameof(InvalidOperationException) => new InvalidOperationException("Test invalid operation exception"),
            nameof(OutOfMemoryException) => new OutOfMemoryException("Test out of memory"),
            _ => new InvalidOperationException("Test exception")
        };
    }

    private class TestMessage
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
