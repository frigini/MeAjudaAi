using FluentAssertions;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;


namespace MeAjudaAi.Shared.Tests.Unit.Messaging;

/// <summary>
/// Testes para Options de Messaging (ServiceBusOptions, RabbitMqOptions, MessageBusOptions)
/// </summary>
public class MessagingExtensionsTests
{


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
        options.DefaultQueueName.Should().Be("meajudaai-events");
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

    [Fact]
    public async Task EnsureMessagingInfrastructureAsync_WhenManagerNotRegistered_ShouldLogWarningAndReturn()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Registrar IHostEnvironment com ambiente de Desenvolvimento para exercer o branch correto
        var hostEnvironment = new MockHostEnvironment("Development");
        services.AddSingleton<IHostEnvironment>(hostEnvironment);
        
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Messaging:Enabled"] = "true"
            }).Build());
        
        // Usar mock verificável para validar que o warning foi logado
        var loggerMock = new Mock<ILogger<MessagingConfiguration>>();
        services.AddSingleton(loggerMock.Object);
        // IRabbitMqInfrastructureManager intencionalmente NÃO registrado

        var serviceProvider = services.BuildServiceProvider();
        var hostMock = new Mock<Microsoft.Extensions.Hosting.IHost>();
        hostMock.Setup(h => h.Services).Returns(serviceProvider);

        // Act
        await hostMock.Object.EnsureMessagingInfrastructureAsync();

        // Assert - verificar que o warning foi logado
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
