using FluentAssertions;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.ServiceBus;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging;

/// <summary>
/// Testes para Options de Messaging (ServiceBusOptions, RabbitMqOptions, MessageBusOptions)
/// </summary>
public class MessagingExtensionsTests
{
    [Fact]
    public void ServiceBusOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new ServiceBusOptions();

        // Assert
        options.DefaultTopicName.Should().Be("MeAjudaAi-events");
        options.ConnectionString.Should().BeEmpty();
        options.DomainTopics.Should().ContainKey("Users");
        options.DomainTopics["Users"].Should().Be("users-events");
    }

    [Fact]
    public void ServiceBusOptions_GetTopicForDomain_ExistingDomain_ShouldReturnCorrectTopic()
    {
        // Arrange
        var options = new ServiceBusOptions();

        // Act
        var topic = options.GetTopicForDomain("Users");

        // Assert
        topic.Should().Be("users-events");
    }

    [Fact]
    public void ServiceBusOptions_GetTopicForDomain_UnknownDomain_ShouldReturnDefaultTopic()
    {
        // Arrange
        var options = new ServiceBusOptions();

        // Act
        var topic = options.GetTopicForDomain("UnknownDomain");

        // Assert
        topic.Should().Be("MeAjudaAi-events");
    }

    [Fact]
    public void RabbitMqOptions_BuildConnectionString_WithVirtualHost_ShouldConstructCorrectly()
    {
        // Arrange
        var options = new RabbitMqOptions
        {
            Host = "customhost",
            Port = 5673,
            Username = "user",
            Password = "pass",
            VirtualHost = "/vhost"
        };

        // Act
        var connectionString = options.BuildConnectionString();

        // Assert
        connectionString.Should().Be("amqp://user:pass@customhost:5673/vhost");
    }

    [Fact]
    public void RabbitMqOptions_BuildConnectionString_WithoutVirtualHost_ShouldConstructCorrectly()
    {
        // Arrange
        var options = new RabbitMqOptions
        {
            Host = "localhost",
            Port = 5672,
            Username = "guest",
            Password = "guest"
        };

        // Act
        var connectionString = options.BuildConnectionString();

        // Assert
        connectionString.Should().Be("amqp://guest:guest@localhost:5672/");
    }

    [Fact]
    public void RabbitMqOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new RabbitMqOptions();

        // Assert
        options.Host.Should().Be("localhost");
        options.Port.Should().Be(5672);
        options.Username.Should().Be("guest");
        options.Password.Should().Be("guest");
        options.DefaultQueueName.Should().Be("MeAjudaAi-events");
    }

    [Fact]
    public void MessageBusOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new MessageBusOptions();

        // Assert
        options.DefaultTimeToLive.Should().Be(TimeSpan.FromDays(1));
        options.MaxConcurrentCalls.Should().Be(1);
        options.MaxDeliveryCount.Should().Be(10);
        options.LockDuration.Should().Be(TimeSpan.FromMinutes(5));
        options.EnableAutoDiscovery.Should().BeTrue();
        options.AssemblyPrefixes.Should().Contain("MeAjudaAi");
    }
}
