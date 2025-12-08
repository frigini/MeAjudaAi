using System.Text.Json;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using MeAjudaAi.Shared.Messaging.Strategy;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging.ServiceBus;

public class ServiceBusMessageBusTests : IDisposable
{
    private readonly Mock<ServiceBusClient> _clientMock;
    private readonly Mock<ITopicStrategySelector> _topicStrategySelectorMock;
    private readonly Mock<ILogger<ServiceBusMessageBus>> _loggerMock;
    private readonly MessageBusOptions _options;
    private readonly ServiceBusMessageBus _messageBus;

    public ServiceBusMessageBusTests()
    {
        _clientMock = new Mock<ServiceBusClient>();
        _topicStrategySelectorMock = new Mock<ITopicStrategySelector>();
        _loggerMock = new Mock<ILogger<ServiceBusMessageBus>>();

        _options = new MessageBusOptions
        {
            MaxConcurrentCalls = 10,
            MaxDeliveryCount = 3,
            LockDuration = TimeSpan.FromMinutes(5),
            DefaultTimeToLive = TimeSpan.FromDays(1),
            QueueNamingConvention = type => $"queue-{type.Name.ToLowerInvariant()}",
            SubscriptionNamingConvention = type => $"sub-{type.Name.ToLowerInvariant()}"
        };

        _messageBus = new ServiceBusMessageBus(
            _clientMock.Object,
            _topicStrategySelectorMock.Object,
            _options,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _messageBus?.DisposeAsync().AsTask().Wait();
        GC.SuppressFinalize(this);
    }

    // Test messages
    public record TestMessage(string Data);
    public record TestCommand(string Action);

    public class TestIntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string EventType { get; init; } = "TestEvent";
        public string Source { get; init; } = "TestService";
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public string Payload { get; init; } = "test";
    }

    [Fact]
    public async Task SendAsync_WithMessage_ShouldSendToQueue()
    {
        // Arrange
        var senderMock = new Mock<ServiceBusSender>();
        _clientMock.Setup(c => c.CreateSender(It.IsAny<string>()))
            .Returns(senderMock.Object);

        var message = new TestMessage("test data");
        ServiceBusMessage? sentMessage = null;

        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceBusMessage, CancellationToken>((msg, _) => sentMessage = msg)
            .Returns(Task.CompletedTask);

        // Act
        await _messageBus.SendAsync(message);

        // Assert
        senderMock.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        sentMessage.Should().NotBeNull();
        sentMessage!.ContentType.Should().Be("application/json");
        sentMessage.Subject.Should().Be("TestMessage");
        sentMessage.ApplicationProperties["MessageType"].Should().Be("TestMessage");
        sentMessage.MessageId.Should().NotBeNullOrEmpty();
        sentMessage.TimeToLive.Should().Be(_options.DefaultTimeToLive);
    }

    [Fact]
    public async Task SendAsync_WithCustomQueueName_ShouldUseProvidedQueueName()
    {
        // Arrange
        var customQueueName = "custom-queue";
        var senderMock = new Mock<ServiceBusSender>();
        string? actualQueueName = null;

        _clientMock.Setup(c => c.CreateSender(It.IsAny<string>()))
            .Callback<string>(queueName => actualQueueName = queueName)
            .Returns(senderMock.Object);

        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var message = new TestMessage("test");

        // Act
        await _messageBus.SendAsync(message, customQueueName);

        // Assert
        actualQueueName.Should().Be(customQueueName);
    }

    [Fact]
    public async Task SendAsync_WithDefaultQueueName_ShouldUseNamingConvention()
    {
        // Arrange
        var senderMock = new Mock<ServiceBusSender>();
        string? actualQueueName = null;

        _clientMock.Setup(c => c.CreateSender(It.IsAny<string>()))
            .Callback<string>(queueName => actualQueueName = queueName)
            .Returns(senderMock.Object);

        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var message = new TestCommand("execute");

        // Act
        await _messageBus.SendAsync(message);

        // Assert
        actualQueueName.Should().Be("queue-testcommand"); // From naming convention
    }

    [Fact]
    public async Task SendAsync_WhenSenderThrows_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var senderMock = new Mock<ServiceBusSender>();
        _clientMock.Setup(c => c.CreateSender(It.IsAny<string>()))
            .Returns(senderMock.Object);

        var expectedException = new InvalidOperationException("Service Bus error");
        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var message = new TestMessage("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _messageBus.SendAsync(message));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send message")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _messageBus.SendAsync<TestMessage>(null!));
    }

    [Fact]
    public async Task PublishAsync_WithEvent_ShouldPublishToTopic()
    {
        // Arrange
        var topicName = "test-topic";
        var senderMock = new Mock<ServiceBusSender>();

        _topicStrategySelectorMock.Setup(s => s.SelectTopicForEvent<TestMessage>())
            .Returns(topicName);

        _clientMock.Setup(c => c.CreateSender(topicName))
            .Returns(senderMock.Object);

        ServiceBusMessage? sentMessage = null;
        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceBusMessage, CancellationToken>((msg, _) => sentMessage = msg)
            .Returns(Task.CompletedTask);

        var @event = new TestMessage("event data");

        // Act
        await _messageBus.PublishAsync(@event);

        // Assert
        senderMock.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        sentMessage.Should().NotBeNull();
        sentMessage!.Subject.Should().Be("TestMessage");
    }

    [Fact]
    public async Task PublishAsync_WithIntegrationEvent_ShouldIncludeEventMetadata()
    {
        // Arrange
        var topicName = "integration-events";
        var senderMock = new Mock<ServiceBusSender>();

        _topicStrategySelectorMock.Setup(s => s.SelectTopicForEvent<TestIntegrationEvent>())
            .Returns(topicName);

        _clientMock.Setup(c => c.CreateSender(topicName))
            .Returns(senderMock.Object);

        ServiceBusMessage? sentMessage = null;
        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceBusMessage, CancellationToken>((msg, _) => sentMessage = msg)
            .Returns(Task.CompletedTask);

        var integrationEvent = new TestIntegrationEvent();

        // Act
        await _messageBus.PublishAsync(integrationEvent);

        // Assert
        sentMessage.Should().NotBeNull();
        sentMessage!.ApplicationProperties["Source"].Should().Be("TestService");
        sentMessage.ApplicationProperties["EventId"].Should().Be(integrationEvent.Id);
        sentMessage.ApplicationProperties["EventType"].Should().Be("TestEvent");
        sentMessage.ApplicationProperties["OccurredAt"].Should().BeOfType<DateTime>();
    }

    [Fact]
    public async Task PublishAsync_WithCustomTopicName_ShouldUseProvidedTopicName()
    {
        // Arrange
        var customTopicName = "custom-topic";
        var senderMock = new Mock<ServiceBusSender>();
        string? actualTopicName = null;

        _clientMock.Setup(c => c.CreateSender(It.IsAny<string>()))
            .Callback<string>(topicName => actualTopicName = topicName)
            .Returns(senderMock.Object);

        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var @event = new TestMessage("test");

        // Act
        await _messageBus.PublishAsync(@event, customTopicName);

        // Assert
        actualTopicName.Should().Be(customTopicName);
        _topicStrategySelectorMock.Verify(s => s.SelectTopicForEvent<TestMessage>(), Times.Never);
    }

    [Fact]
    public async Task PublishAsync_WhenSenderThrows_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var topicName = "test-topic";
        var senderMock = new Mock<ServiceBusSender>();

        _topicStrategySelectorMock.Setup(s => s.SelectTopicForEvent<TestMessage>())
            .Returns(topicName);

        _clientMock.Setup(c => c.CreateSender(topicName))
            .Returns(senderMock.Object);

        var expectedException = new InvalidOperationException("Publish failed");
        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var @event = new TestMessage("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _messageBus.PublishAsync(@event));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to publish event")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _messageBus.PublishAsync<TestMessage>(null!));
    }

    [Fact]
    public async Task SendAsync_WithCancellationToken_ShouldPassTokenToSender()
    {
        // Arrange
        var senderMock = new Mock<ServiceBusSender>();
        _clientMock.Setup(c => c.CreateSender(It.IsAny<string>()))
            .Returns(senderMock.Object);

        using var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceBusMessage, CancellationToken>((_, ct) => receivedToken = ct)
            .Returns(Task.CompletedTask);

        var message = new TestMessage("test");

        // Act
        await _messageBus.SendAsync(message, cancellationToken: cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task PublishAsync_WithCancellationToken_ShouldPassTokenToSender()
    {
        // Arrange
        var topicName = "test-topic";
        var senderMock = new Mock<ServiceBusSender>();

        _topicStrategySelectorMock.Setup(s => s.SelectTopicForEvent<TestMessage>())
            .Returns(topicName);

        _clientMock.Setup(c => c.CreateSender(topicName))
            .Returns(senderMock.Object);

        using var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceBusMessage, CancellationToken>((_, ct) => receivedToken = ct)
            .Returns(Task.CompletedTask);

        var @event = new TestMessage("test");

        // Act
        await _messageBus.PublishAsync(@event, cancellationToken: cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task SendAsync_MultipleCalls_ShouldReuseSameSender()
    {
        // Arrange
        var queueName = "queue-testmessage";
        var senderMock = new Mock<ServiceBusSender>();

        _clientMock.Setup(c => c.CreateSender(queueName))
            .Returns(senderMock.Object);

        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _messageBus.SendAsync(new TestMessage("msg1"));
        await _messageBus.SendAsync(new TestMessage("msg2"));
        await _messageBus.SendAsync(new TestMessage("msg3"));

        // Assert
        _clientMock.Verify(c => c.CreateSender(queueName), Times.Once); // Sender reused
        senderMock.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeSendersAndClient()
    {
        // Arrange
        var senderMock = new Mock<ServiceBusSender>();
        _clientMock.Setup(c => c.CreateSender(It.IsAny<string>()))
            .Returns(senderMock.Object);

        senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _messageBus.SendAsync(new TestMessage("test"));

        // Act
        await _messageBus.DisposeAsync();

        // Assert
        senderMock.Verify(s => s.DisposeAsync(), Times.Once);
        _clientMock.Verify(c => c.DisposeAsync(), Times.Once);
    }
}
