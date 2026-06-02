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
    public void AddMessaging_WhenDisabled_ShouldRegisterOnlyNoOpMessageBus()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ILogger<MeAjudaAi.Shared.Messaging.NoOp.NoOpMessageBus>>());
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Messaging:Enabled"] = "false"
            }).Build();
        var env = new MockHostEnvironment("Development");

        services.AddMessaging(configuration, env);

        var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<IMessageBus>();
        bus.Should().BeOfType<MeAjudaAi.Shared.Messaging.NoOp.NoOpMessageBus>();
    }

    [Fact]
    public async Task EnsureMessagingInfrastructureAsync_WhenDisabled_ShouldReturnEarlyWithoutThrow()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Messaging:Enabled"] = "false"
            }).Build());

        var provider = services.BuildServiceProvider();
        var hostMock = new Mock<Microsoft.Extensions.Hosting.IHost>();
        hostMock.Setup(h => h.Services).Returns(provider);

        // Não deve lançar exceção nem exigir IRabbitMqInfrastructureManager
        var act = () => hostMock.Object.EnsureMessagingInfrastructureAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureMessagingInfrastructureAsync_WhenTestingEnvironment_ShouldReturnEarlyWithoutThrow()
    {
        using var envScope = new EnvironmentVariableScope(
            ("ASPNETCORE_ENVIRONMENT", "Testing"),
            ("DOTNET_ENVIRONMENT", null),
            ("INTEGRATION_TESTS", null));

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new MockHostEnvironment("Testing"));
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Messaging:Enabled"] = "true"
            }).Build());
        services.AddSingleton(Mock.Of<ILogger<MessagingConfiguration>>());
        // IRabbitMqInfrastructureManager intencionalmente ausente

        var provider = services.BuildServiceProvider();
        var hostMock = new Mock<Microsoft.Extensions.Hosting.IHost>();
        hostMock.Setup(h => h.Services).Returns(provider);

        var act = () => hostMock.Object.EnsureMessagingInfrastructureAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureMessagingInfrastructureAsync_WhenManagerRegistered_ShouldCallEnsureInfrastructure()
    {
        using var envScope = new EnvironmentVariableScope(
            ("ASPNETCORE_ENVIRONMENT", "Development"),
            ("DOTNET_ENVIRONMENT", "Development"),
            ("INTEGRATION_TESTS", null));

        var mockManager = new Mock<MeAjudaAi.Shared.Messaging.RabbitMq.IRabbitMqInfrastructureManager>();
        mockManager.Setup(m => m.EnsureInfrastructureAsync()).Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new MockHostEnvironment("Development"));
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Messaging:Enabled"] = "true"
            }).Build());
        services.AddSingleton(Mock.Of<ILogger<MessagingConfiguration>>());
        services.AddSingleton(mockManager.Object);

        var provider = services.BuildServiceProvider();
        var hostMock = new Mock<Microsoft.Extensions.Hosting.IHost>();
        hostMock.Setup(h => h.Services).Returns(provider);

        await hostMock.Object.EnsureMessagingInfrastructureAsync();

        mockManager.Verify(m => m.EnsureInfrastructureAsync(), Times.Once);
    }

    [Fact]
    public async Task EnsureMessagingInfrastructureAsync_WhenManagerThrows_ShouldRethrow()
    {
        using var envScope = new EnvironmentVariableScope(
            ("ASPNETCORE_ENVIRONMENT", "Development"),
            ("DOTNET_ENVIRONMENT", "Development"),
            ("INTEGRATION_TESTS", null));

        var mockManager = new Mock<MeAjudaAi.Shared.Messaging.RabbitMq.IRabbitMqInfrastructureManager>();
        mockManager.Setup(m => m.EnsureInfrastructureAsync())
            .ThrowsAsync(new InvalidOperationException("RabbitMQ unreachable"));

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new MockHostEnvironment("Development"));
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Messaging:Enabled"] = "true"
            }).Build());
        services.AddSingleton(Mock.Of<ILogger<MessagingConfiguration>>());
        services.AddSingleton(mockManager.Object);

        var provider = services.BuildServiceProvider();
        var hostMock = new Mock<Microsoft.Extensions.Hosting.IHost>();
        hostMock.Setup(h => h.Services).Returns(provider);

        var act = () => hostMock.Object.EnsureMessagingInfrastructureAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*RabbitMQ unreachable*");
    }

    [Fact]
    public void AddMessaging_WhenAspireConnectionStringProvided_ShouldUseFallback()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Messaging:Enabled"] = "true",
                ["ConnectionStrings:rabbitmq"] = "amqp://aspire-host:5672/",
                // Sem Messaging:RabbitMQ:ConnectionString explícita
            }).Build();
        var env = new MockHostEnvironment("Testing"); // Testing evita registro do Rebus

        services.AddMessaging(configuration, env);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<RabbitMqOptions>();
        options.ConnectionString.Should().Be("amqp://aspire-host:5672/");
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
    public async Task EnsureMessagingInfrastructureAsync_WhenManagerNotRegistered_ShouldThrowException()
    {
        using var envScope = new EnvironmentVariableScope(
            ("ASPNETCORE_ENVIRONMENT", "Development"),
            ("DOTNET_ENVIRONMENT", "Development"), 
            ("INTEGRATION_TESTS", null));

        var services = new ServiceCollection();
        
        // Registrar IHostEnvironment com ambiente de Desenvolvimento para exercer o branch correto
        var hostEnvironment = new MockHostEnvironment("Development");
        services.AddSingleton<IHostEnvironment>(hostEnvironment);
        
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Messaging:Enabled"] = "true"
            }).Build());
        
        services.AddSingleton(Mock.Of<ILogger<MessagingConfiguration>>());
        // IRabbitMqInfrastructureManager intencionalmente NÃO registrado

        var serviceProvider = services.BuildServiceProvider();
        var hostMock = new Mock<Microsoft.Extensions.Hosting.IHost>();
        hostMock.Setup(h => h.Services).Returns(serviceProvider);

        // Act & Assert - agora deve lançar exceção para fail-fast
        var act = () => hostMock.Object.EnsureMessagingInfrastructureAsync();
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*IRabbitMqInfrastructureManager*not registered*");
    }
}

internal sealed class EnvironmentVariableScope : IDisposable
{
    private static readonly object _lock = new();
    private readonly (string Name, string? OriginalValue)[] _variables;
    private bool _disposed;

    public EnvironmentVariableScope(params (string Name, string? NewValue)[] variables)
    {
        _variables = variables.Select(v => (v.Name, Environment.GetEnvironmentVariable(v.Name))).ToArray();
        
        lock (_lock)
        {
            foreach (var v in variables)
            {
                Environment.SetEnvironmentVariable(v.Name, v.NewValue);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        lock (_lock)
        {
            foreach (var (name, originalValue) in _variables)
            {
                Environment.SetEnvironmentVariable(name, originalValue);
            }
        }
    }
}
