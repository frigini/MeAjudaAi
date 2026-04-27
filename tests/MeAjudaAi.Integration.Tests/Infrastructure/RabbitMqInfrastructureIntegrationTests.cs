using Testcontainers.RabbitMq;
using RabbitMQ.Client;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using FluentAssertions;
using MeAjudaAi.Shared.Messaging;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

[Trait("Category", "Integration")]
[Trait("Area", "Infrastructure")]
[Trait("Messaging", "RabbitMQ")]
public sealed class RabbitMqInfrastructureIntegrationTests : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder("rabbitmq:4.0-management")
        .Build();

    public ValueTask InitializeAsync() => new(_container.StartAsync());
    public ValueTask DisposeAsync() => _container.DisposeAsync();

    [Fact]
    public async Task EnsureInfrastructureAsync_ShouldDeclareEverythingInRealRabbitMq()
    {
        // Arrange
        var connectionString = _container.GetConnectionString();
        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
        var connection = await factory.CreateConnectionAsync();

        var options = new RabbitMqOptions
        {
            DefaultQueueName = "test-integration-queue",
            DomainQueues = { ["Auth"] = "auth-integration-queue" }
        };

        var registryMock = new Mock<IEventTypeRegistry>();
        registryMock.Setup(r => r.GetAllEventTypesAsync()).ReturnsAsync(new List<Type>());

        var sut = new RabbitMqInfrastructureManager(connection, options, registryMock.Object, NullLogger<RabbitMqInfrastructureManager>.Instance);

        // Act
        await sut.EnsureInfrastructureAsync();

        // Assert
        // Verify by trying to declare passively
        var channel = await connection.CreateChannelAsync();
        
        var defaultQueue = await channel.QueueDeclarePassiveAsync("test-integration-queue");
        defaultQueue.QueueName.Should().Be("test-integration-queue");

        var domainQueue = await channel.QueueDeclarePassiveAsync("auth-integration-queue");
        domainQueue.QueueName.Should().Be("auth-integration-queue");

        // Verify exchange (constructed as defaultQueueName + ".exchange")
        await channel.ExchangeDeclarePassiveAsync("test-integration-queue.exchange");
        
        await connection.CloseAsync();
    }
}
