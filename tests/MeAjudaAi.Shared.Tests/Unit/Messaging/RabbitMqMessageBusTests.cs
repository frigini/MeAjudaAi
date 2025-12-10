using FluentAssertions;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging;

/// <summary>
/// Unit tests for <see cref="RabbitMqMessageBus"/>.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Messaging")]
public class RabbitMqMessageBusTests
{
    private readonly Mock<ILogger<RabbitMqMessageBus>> _loggerMock;
    private readonly RabbitMqOptions _options;

    public RabbitMqMessageBusTests()
    {
        _loggerMock = new Mock<ILogger<RabbitMqMessageBus>>();
        _options = new RabbitMqOptions
        {
            Host = "localhost",
            Port = 5672,
            Username = "guest",
            Password = "guest",
            DefaultQueueName = "test-queue"
        };
    }

    [Fact]
    public async Task SendAsync_WithDefaultQueue_ShouldLogWithDefaultQueueName()
    {
        // Arrange
        var messageBus = new RabbitMqMessageBus(_options, _loggerMock.Object);
        var message = new ProviderRegisteredIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Provider",
            "Individual",
            "test@example.com");

        // Act
        await messageBus.SendAsync(message, null, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("test-queue")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithCustomQueue_ShouldLogWithCustomQueueName()
    {
        // Arrange
        var messageBus = new RabbitMqMessageBus(_options, _loggerMock.Object);
        var message = new ProviderRegisteredIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Provider",
            "Individual",
            "test@example.com");

        // Act
        await messageBus.SendAsync(message, "custom-queue", CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("custom-queue")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithDefaultTopic_ShouldLogWithDefaultQueueName()
    {
        // Arrange
        var messageBus = new RabbitMqMessageBus(_options, _loggerMock.Object);
        var @event = new ProviderRegisteredIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Provider",
            "Individual",
            "test@example.com");

        // Act
        await messageBus.PublishAsync(@event, null, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("test-queue")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithCustomTopic_ShouldLogWithCustomTopicName()
    {
        // Arrange
        var messageBus = new RabbitMqMessageBus(_options, _loggerMock.Object);
        var @event = new ProviderRegisteredIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Provider",
            "Individual",
            "test@example.com");

        // Act
        await messageBus.PublishAsync(@event, "custom-topic", CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("custom-topic")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_WithDefaultSubscription_ShouldLogWithDefaultSubscriptionName()
    {
        // Arrange
        var messageBus = new RabbitMqMessageBus(_options, _loggerMock.Object);
        Task Handler(ProviderRegisteredIntegrationEvent msg, CancellationToken ct) => Task.CompletedTask;

        // Act
        await messageBus.SubscribeAsync<ProviderRegisteredIntegrationEvent>(
            Handler,
            null,
            CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ProviderRegisteredIntegrationEvent-subscription")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_WithCustomSubscription_ShouldLogWithCustomSubscriptionName()
    {
        // Arrange
        var messageBus = new RabbitMqMessageBus(_options, _loggerMock.Object);
        Task Handler(ProviderRegisteredIntegrationEvent msg, CancellationToken ct) => Task.CompletedTask;

        // Act
        await messageBus.SubscribeAsync<ProviderRegisteredIntegrationEvent>(
            Handler,
            "custom-subscription",
            CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("custom-subscription")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldLogDebugWithMessageContent()
    {
        // Arrange
        var messageBus = new RabbitMqMessageBus(_options, _loggerMock.Object);
        var message = new ProviderRegisteredIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Provider",
            "Individual",
            "test@example.com");

        // Act
        await messageBus.SendAsync(message, null, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RabbitMQ Message Content")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldLogDebugWithEventContent()
    {
        // Arrange
        var messageBus = new RabbitMqMessageBus(_options, _loggerMock.Object);
        var @event = new ProviderRegisteredIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Provider",
            "Individual",
            "test@example.com");

        // Act
        await messageBus.PublishAsync(@event, null, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RabbitMQ Event Content")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
