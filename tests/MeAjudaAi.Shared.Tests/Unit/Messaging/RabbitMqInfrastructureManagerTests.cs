using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using RabbitMQ.Client;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging;

[Trait("Category", "Unit")]
public class RabbitMqInfrastructureManagerTests
{
    private readonly Mock<IConnection> _connectionMock = new();
    private readonly Mock<IChannel> _channelMock = new();
    private readonly Mock<IEventTypeRegistry> _registryMock = new();
    private readonly Mock<ILogger<RabbitMqInfrastructureManager>> _loggerMock = new();
    private readonly RabbitMqOptions _options = new() { DefaultQueueName = "test-queue" };
    private readonly RabbitMqInfrastructureManager _sut;

    private readonly List<string> _declaredQueues = new();

    public RabbitMqInfrastructureManagerTests()
    {
        _connectionMock.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_channelMock.Object);

        // Capture queue declarations (8-parameter signature for QueueDeclareAsync)
        _channelMock.Setup(c => c.QueueDeclareAsync(
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<bool>(), 
                It.IsAny<bool>(), 
                It.IsAny<IDictionary<string, object?>>(), 
                It.IsAny<bool>(), 
                It.IsAny<bool>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, bool d, bool e, bool a, IDictionary<string, object?> args, bool p, bool nw, CancellationToken ct) => 
            {
                _declaredQueues.Add(name);
                return new QueueDeclareOk(name, 0, 0);
            });

        // Default setup for ExchangeDeclareAsync and QueueBindAsync (return completed tasks)
        _channelMock.Setup(c => c.ExchangeDeclareAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object?>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _channelMock.Setup(c => c.QueueBindAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, object?>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new RabbitMqInfrastructureManager(_connectionMock.Object, _options, _registryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task EnsureInfrastructureAsync_ShouldDeclareQueuesExchangesAndBindings()
    {
        // Arrange
        _registryMock.Setup(r => r.GetAllEventTypesAsync()).ReturnsAsync(new List<Type> { typeof(TestEvent) });
        _options.DomainQueues["Users"] = "users-queue";

        // Act
        await _sut.EnsureInfrastructureAsync();

        // Assert - Queues
        _declaredQueues.Should().Contain("test-queue");
        _declaredQueues.Should().Contain("users-queue");

        // Assert - Exchange was declared
        _channelMock.Verify(c => c.ExchangeDeclareAsync(
            "test-queue.exchange",
            ExchangeType.Topic,
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<IDictionary<string, object?>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Assert - Bindings were created using FullName as routing key
        var expectedRoutingKey = typeof(TestEvent).FullName!;
        _channelMock.Verify(c => c.QueueBindAsync(
            "test-queue",
            "test-queue.exchange",
            expectedRoutingKey,
            It.IsAny<IDictionary<string, object?>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _channelMock.Verify(c => c.QueueBindAsync(
            "users-queue",
            "test-queue.exchange",
            expectedRoutingKey,
            It.IsAny<IDictionary<string, object?>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateQueueAsync_ShouldCallChannelCorrectly()
    {
        // Act
        await _sut.CreateQueueAsync("my-queue", false);

        // Assert
        _declaredQueues.Should().Contain("my-queue");
    }

    private class TestEvent { }
}
