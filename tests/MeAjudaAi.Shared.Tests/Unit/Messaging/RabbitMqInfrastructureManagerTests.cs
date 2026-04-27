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
    private readonly List<bool> _declaredQueueDurables = new();

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
                _declaredQueueDurables.Add(d);
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
        var index = _declaredQueues.IndexOf("my-queue");
        _declaredQueueDurables[index].Should().BeFalse();
    }

    [Fact]
    public async Task CreateQueueAsync_WithDurableTrue_ShouldPassDurableTrueToChannel()
    {
        // Act
        await _sut.CreateQueueAsync("durable-queue", true);

        // Assert
        _declaredQueues.Should().Contain("durable-queue");
        var index = _declaredQueues.IndexOf("durable-queue");
        _declaredQueueDurables[index].Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_WithOpenChannel_ShouldDisposeChannel()
    {
        // Arrange – garante que o canal é criado
        _registryMock.Setup(r => r.GetAllEventTypesAsync()).ReturnsAsync(new List<Type>());
        await _sut.EnsureInfrastructureAsync();

        // Act
        await _sut.DisposeAsync();

        // Assert
        _channelMock.Verify(c => c.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_WithoutChannel_ShouldNotThrow()
    {
        // Act & Assert — nenhum canal criado, não deve lançar
        var act = async () => await _sut.DisposeAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureInfrastructureAsync_WithNullFullNameEvent_ShouldLogWarningAndSkip()
    {
        // Arrange – tipo sem FullName (anonymous/nested edge-case simulado via mock)
        var typeMock = new Mock<Type>();
        typeMock.Setup(t => t.FullName).Returns((string?)null);
        _registryMock.Setup(r => r.GetAllEventTypesAsync())
            .ReturnsAsync(new List<Type> { typeMock.Object });

        // Act
        await _sut.EnsureInfrastructureAsync();

        // Assert – nenhum bind deve ter ocorrido
        _channelMock.Verify(c => c.QueueBindAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IDictionary<string, object?>>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsureInfrastructureAsync_WhenChannelFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _connectionMock.Setup(c => c.CreateChannelAsync(
            It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("broker unavailable"));

        // Act & Assert
        await _sut.Invoking(s => s.EnsureInfrastructureAsync())
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to create RabbitMQ infrastructure*");
    }

    [Fact]
    public async Task GetChannelAsync_WhenCalledTwice_ShouldReuseOpenChannel()
    {
        // Arrange – configura IsOpen = true após primeira criação
        _channelMock.Setup(c => c.IsOpen).Returns(true);
        _registryMock.Setup(r => r.GetAllEventTypesAsync()).ReturnsAsync(new List<Type>());

        // Act – duas chamadas a EnsureInfrastructureAsync
        await _sut.EnsureInfrastructureAsync();
        await _sut.EnsureInfrastructureAsync();

        // Assert – canal criado apenas uma vez
        _connectionMock.Verify(c => c.CreateChannelAsync(
            It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private class TestEvent { }
}
