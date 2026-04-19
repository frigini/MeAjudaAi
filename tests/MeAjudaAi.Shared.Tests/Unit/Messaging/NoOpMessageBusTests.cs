using MeAjudaAi.Shared.Messaging.NoOp;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging;

[Trait("Category", "Unit")]
public class NoOpMessageBusTests
{
    private readonly Mock<ILogger<NoOpMessageBus>> _loggerMock = new();
    private readonly NoOpMessageBus _sut;

    public NoOpMessageBusTests()
    {
        _sut = new NoOpMessageBus(_loggerMock.Object);
    }

    [Fact]
    public async Task SendAsync_ShouldCompleteWithoutException()
    {
        // Arrange
        var message = new { Id = 1 };

        // Act
        var act = () => _sut.SendAsync(message, "test-queue");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_ShouldCompleteWithoutException()
    {
        // Arrange
        var @event = new { Id = 1 };

        // Act
        var act = () => _sut.PublishAsync(@event, "test-topic");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SubscribeAsync_WithNullHandler_ShouldCompleteWithoutException()
    {
        // Act
        var act = () => _sut.SubscribeAsync<string>(null, "test-sub");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SubscribeAsync_WithHandler_ShouldCompleteWithoutException()
    {
        // Arrange
        Func<string, CancellationToken, Task> handler = (msg, ct) => Task.CompletedTask;

        // Act
        var act = () => _sut.SubscribeAsync(handler, "test-sub");

        // Assert
        await act.Should().NotThrowAsync();
    }
}
