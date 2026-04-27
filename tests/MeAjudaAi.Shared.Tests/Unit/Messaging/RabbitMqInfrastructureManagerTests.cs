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

    public RabbitMqInfrastructureManagerTests()
    {
        _connectionMock.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_channelMock.Object);
        
        _sut = new RabbitMqInfrastructureManager(_connectionMock.Object, _options, _registryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task EnsureInfrastructureAsync_ShouldDeclareQueuesAndExchanges()
    {
        // Arrange
        _registryMock.Setup(r => r.GetAllEventTypesAsync()).ReturnsAsync(new List<Type> { typeof(TestEvent) });
        _options.DomainQueues["Users"] = "users-queue";

        var declaredQueues = new List<string>();
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
                declaredQueues.Add(name);
                return new QueueDeclareOk(name, 0, 0);
            });

        // Act
        await _sut.EnsureInfrastructureAsync();

        // Assert
        declaredQueues.Should().Contain("test-queue");
        declaredQueues.Should().Contain("users-queue");
    }

    [Fact]
    public async Task CreateQueueAsync_ShouldCallChannelCorrectly()
    {
        // Arrange
        string? capturedQueue = null;
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
                capturedQueue = name;
                return new QueueDeclareOk(name, 0, 0);
            });

        // Act
        await _sut.CreateQueueAsync("my-queue", false);

        // Assert
        capturedQueue.Should().Be("my-queue");
    }

    private class TestEvent { }
}
