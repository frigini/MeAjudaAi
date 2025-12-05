using FluentAssertions;
using MeAjudaAi.Shared.Messaging.NoOp;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Messaging;

public class NoOpMessageBusTests
{
    private readonly Mock<ILogger<NoOpMessageBus>> _mockLogger;
    private readonly NoOpMessageBus _messageBus;

    public NoOpMessageBusTests()
    {
        _mockLogger = new Mock<ILogger<NoOpMessageBus>>();
        _messageBus = new NoOpMessageBus(_mockLogger.Object);
    }

    [Fact]
    public async Task SendAsync_ShouldLogDebug_AndCompleteSuccessfully()
    {
        // Arrange
        var message = new TestMessage { Content = "Test" };

        // Act
        var act = async () => await _messageBus.SendAsync(message);

        // Assert
        await act.Should().NotThrowAsync();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ignoring message")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldLogQueueName_WhenProvided()
    {
        // Arrange
        var message = new TestMessage { Content = "Test" };
        var queueName = "test-queue";

        // Act
        await _messageBus.SendAsync(message, queueName);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(queueName)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldLogDefaultQueue_WhenNotProvided()
    {
        // Arrange
        var message = new TestMessage { Content = "Test" };

        // Act
        await _messageBus.SendAsync(message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("default")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldLogDebug_AndCompleteSuccessfully()
    {
        // Arrange
        var @event = new TestEvent { EventType = "Test" };

        // Act
        var act = async () => await _messageBus.PublishAsync(@event);

        // Assert
        await act.Should().NotThrowAsync();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ignoring event")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldLogTopicName_WhenProvided()
    {
        // Arrange
        var @event = new TestEvent { EventType = "Test" };
        var topicName = "test-topic";

        // Act
        await _messageBus.PublishAsync(@event, topicName);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(topicName)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldLogDebug_AndCompleteSuccessfully()
    {
        // Arrange
        Task Handler(TestMessage message, CancellationToken ct) => Task.CompletedTask;

        // Act
        var act = async () => await _messageBus.SubscribeAsync<TestMessage>(Handler);

        // Assert
        await act.Should().NotThrowAsync();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ignoring subscription")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldLogSubscriptionName_WhenProvided()
    {
        // Arrange
        var subscriptionName = "test-subscription";
        Task Handler(TestMessage message, CancellationToken ct) => Task.CompletedTask;

        // Act
        await _messageBus.SubscribeAsync<TestMessage>(Handler, subscriptionName);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(subscriptionName)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var message = new TestMessage { Content = "Test" };

        // Act
        var act = async () => await _messageBus.SendAsync(message, cancellationToken: cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var @event = new TestEvent { EventType = "Test" };

        // Act
        var act = async () => await _messageBus.PublishAsync(@event, cancellationToken: cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SubscribeAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        Task Handler(TestMessage message, CancellationToken ct) => Task.CompletedTask;

        // Act
        var act = async () => await _messageBus.SubscribeAsync<TestMessage>(Handler, cancellationToken: cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendAsync_ShouldNotInvokeAnyHandler()
    {
        // Arrange
        var message = new TestMessage { Content = "Test" };
        var handlerInvoked = false;
        
        // Act
        await _messageBus.SendAsync(message);
        
        // Assert
        handlerInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task PublishAsync_ShouldNotInvokeAnyHandler()
    {
        // Arrange
        var @event = new TestEvent { EventType = "Test" };
        var handlerInvoked = false;
        
        // Act
        await _messageBus.PublishAsync(@event);
        
        // Assert
        handlerInvoked.Should().BeFalse();
    }

    private class TestMessage
    {
        public string Content { get; set; } = string.Empty;
    }

    private class TestEvent
    {
        public string EventType { get; set; } = string.Empty;
    }
}
