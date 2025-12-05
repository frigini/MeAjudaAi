using FluentAssertions;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Messaging;

public class RabbitMqMessageBusTests
{
    private readonly Mock<ILogger<RabbitMqMessageBus>> _mockLogger;
    private readonly RabbitMqOptions _options;
    private readonly RabbitMqMessageBus _messageBus;

    public RabbitMqMessageBusTests()
    {
        _mockLogger = new Mock<ILogger<RabbitMqMessageBus>>();
        _options = new RabbitMqOptions
        {
            DefaultQueueName = "default-queue",
            HostName = "localhost",
            Port = 5672
        };
        _messageBus = new RabbitMqMessageBus(_options, _mockLogger.Object);
    }

    [Fact]
    public async Task SendAsync_ShouldLogMessage_WithDefaultQueue()
    {
        // Arrange
        var message = new TestMessage { Content = "Test content" };

        // Act
        await _messageBus.SendAsync(message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMessage") && v.ToString()!.Contains("default-queue")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldLogMessage_WithCustomQueue()
    {
        // Arrange
        var message = new TestMessage { Content = "Test content" };
        var customQueue = "custom-queue";

        // Act
        await _messageBus.SendAsync(message, customQueue);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(customQueue)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldLogDebugMessage_WithSerializedContent()
    {
        // Arrange
        var message = new TestMessage { Content = "Test content" };

        // Act
        await _messageBus.SendAsync(message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RabbitMQ Message Content")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldLogEvent_WithDefaultTopic()
    {
        // Arrange
        var @event = new TestEvent { EventType = "TestEvent" };

        // Act
        await _messageBus.PublishAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestEvent") && v.ToString()!.Contains("default-queue")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldLogEvent_WithCustomTopic()
    {
        // Arrange
        var @event = new TestEvent { EventType = "TestEvent" };
        var customTopic = "custom-topic";

        // Act
        await _messageBus.PublishAsync(@event, customTopic);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(customTopic)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldLogDebugEvent_WithSerializedContent()
    {
        // Arrange
        var @event = new TestEvent { EventType = "TestEvent" };

        // Act
        await _messageBus.PublishAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RabbitMQ Event Content")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldLogSubscription_WithDefaultSubscriptionName()
    {
        // Arrange
        Task Handler(TestMessage message, CancellationToken ct) => Task.CompletedTask;

        // Act
        await _messageBus.SubscribeAsync<TestMessage>(Handler);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMessage-subscription")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldLogSubscription_WithCustomSubscriptionName()
    {
        // Arrange
        var customSubscription = "custom-subscription";
        Task Handler(TestMessage message, CancellationToken ct) => Task.CompletedTask;

        // Act
        await _messageBus.SubscribeAsync<TestMessage>(Handler, customSubscription);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(customSubscription)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var message = new TestMessage { Content = "Test" };

        // Act
        var act = async () => await _messageBus.SendAsync(message);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var @event = new TestEvent { EventType = "Test" };

        // Act
        var act = async () => await _messageBus.PublishAsync(@event);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SubscribeAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        Task Handler(TestMessage message, CancellationToken ct) => Task.CompletedTask;

        // Act
        var act = async () => await _messageBus.SubscribeAsync<TestMessage>(Handler);

        // Assert
        await act.Should().NotThrowAsync();
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

    private class TestMessage
    {
        public string Content { get; set; } = string.Empty;
    }

    private class TestEvent
    {
        public string EventType { get; set; } = string.Empty;
    }
}
