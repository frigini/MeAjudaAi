using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Messaging.Rebus;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;
using Xunit;

namespace MeAjudaAi.Shared.Messaging.Tests.Unit.Messaging;

public class RebusMessageBusTests
{
    private readonly Mock<IBus> _busMock;
    private readonly Mock<ILogger<RebusMessageBus>> _loggerMock;
    private readonly RebusMessageBus _messageBus;

    public RebusMessageBusTests()
    {
        _busMock = new Mock<IBus>();
        _loggerMock = new Mock<ILogger<RebusMessageBus>>();
        _messageBus = new RebusMessageBus(_busMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendAsync_ShouldDelegateToBus()
    {
        // Arrange
        var message = new { Data = "test" };

        // Act
        await _messageBus.SendAsync(message);

        // Assert
        _busMock.Verify(x => x.Send(message, null), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldDelegateToBus()
    {
        // Arrange
        var @event = new { Data = "event" };

        // Act
        await _messageBus.PublishAsync(@event);

        // Assert
        _busMock.Verify(x => x.Publish(@event, null), Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldDelegateToBus()
    {
        // Act
        await _messageBus.SubscribeAsync<string>(null!);

        // Assert
        _busMock.Verify(x => x.Subscribe<string>(), Times.Once);
    }
}
