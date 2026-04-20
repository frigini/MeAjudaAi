using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging;

[Trait("Category", "Unit")]
public class RabbitMqInfrastructureManagerTests
{
    private readonly Mock<IEventTypeRegistry> _registryMock = new();
    private readonly Mock<ILogger<RabbitMqInfrastructureManager>> _loggerMock = new();
    private readonly RabbitMqOptions _options = new() { DefaultQueueName = "test-queue" };
    private readonly RabbitMqInfrastructureManager _sut;

    public RabbitMqInfrastructureManagerTests()
    {
        _sut = new RabbitMqInfrastructureManager(_options, _registryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task EnsureInfrastructureAsync_ShouldExecuteCorrectly()
    {
        // Arrange
        _registryMock.Setup(r => r.GetAllEventTypesAsync()).ReturnsAsync(new List<Type> { typeof(string) });
        _options.DomainQueues.Add("Users", "users-queue");

        // Act
        await _sut.EnsureInfrastructureAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating RabbitMQ")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
