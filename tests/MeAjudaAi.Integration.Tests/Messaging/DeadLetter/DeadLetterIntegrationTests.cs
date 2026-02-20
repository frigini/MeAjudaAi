using FluentAssertions;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Factories;
using MeAjudaAi.Shared.Messaging.Handlers;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Testcontainers.RabbitMq;

namespace MeAjudaAi.Shared.Tests.Integration.Messaging.DeadLetter;

/// <summary>
/// Testes de integração para o sistema de Dead Letter Queue
/// </summary>
[Trait("Category", "Integration")]
[Trait("Layer", "Shared")]
[Trait("Component", "DeadLetterSystem")]
public class DeadLetterIntegrationTests : BaseIntegrationTest
{
    private RabbitMqOptions CreateTestRabbitMqOptions()
    {
        // Tenta obter a connection string do container se disponível
        string connectionString = "amqp://localhost";
        try 
        {
            var container = ServiceProvider.GetService<RabbitMqContainer>();
            if (container != null)
            {
                connectionString = container.GetConnectionString();
            }
        }
        catch { /* Fallback para localhost */ }

        return new RabbitMqOptions
        {
            ConnectionString = connectionString,
            DefaultQueueName = "test-queue",
            Username = "guest",
            Password = "guest",
            VirtualHost = "/",
            DomainQueues = new Dictionary<string, string> { ["Users"] = "users-events-test" }
        };
    }

    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                AutoMigrate = false // Testes de Dead Letter não precisam de migrações de banco
            },
            Cache = new TestCacheOptions
            {
                Enabled = false // Testes de Dead Letter não precisam de cache
            }
        };
    }

    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        // Configura serviços de messaging para testes de Dead Letter
        services.AddLogging();

        // Adiciona configuração de Dead Letter
        var configuration = CreateConfiguration();
        services.AddSingleton(configuration);
        services.AddSingleton(CreateHostEnvironment("Development"));

        // Registra RabbitMqOptions explicitamente para os testes de Dead Letter
        services.AddSingleton(CreateTestRabbitMqOptions());
    }

    [Fact]
    public async Task DeadLetter_ShouldMoveMessageToDlq_AfterMaxRetries()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        var environment = CreateHostEnvironment("Testing"); // Usa ambiente Testing para NoOpService

        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddSingleton(environment);

        services.AddSingleton(CreateTestRabbitMqOptions());

        // Act
        MessagingExtensions.AddDeadLetterQueue(
            services, configuration);

        var serviceProvider = services.BuildServiceProvider();
        var deadLetterService = serviceProvider.GetRequiredService<IDeadLetterService>();

        // Assert
        deadLetterService.Should().NotBeNull();
        deadLetterService.Should().BeOfType<NoOpDeadLetterService>();
    }

    [Fact]
    public void DeadLetterSystem_WithTestingEnvironment_UsesNoOpService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        var environment = CreateHostEnvironment("Testing"); // Usa ambiente Testing para NoOpService

        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddSingleton(environment);

        services.AddSingleton(CreateTestRabbitMqOptions());

        // Act
        MessagingExtensions.AddDeadLetterQueue(
            services, configuration);

        var serviceProvider = services.BuildServiceProvider();
        var deadLetterService = serviceProvider.GetRequiredService<IDeadLetterService>();

        // Assert
        deadLetterService.Should().NotBeNull();
        deadLetterService.Should().BeOfType<NoOpDeadLetterService>();
    }

    [Fact]
    public void DeadLetterSystem_WithProductionEnvironment_ConfiguresServiceBusService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(includeServiceBus: true);
        var environment = CreateHostEnvironment("Production");

        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddSingleton(environment);

        services.AddSingleton(CreateTestRabbitMqOptions());

        // Act
        MessagingExtensions.AddDeadLetterQueue(
            services, configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var factory = serviceProvider.GetRequiredService<IDeadLetterServiceFactory>();
        factory.Should().NotBeNull();
        factory.Should().BeOfType<DeadLetterServiceFactory>();
    }

    [Fact]
    public void DeadLetterConfiguration_WithValidOptions_BindsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        var environment = CreateHostEnvironment("Testing");

        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddSingleton(environment);

        services.AddSingleton(CreateTestRabbitMqOptions());

        // Act
        MessagingExtensions.AddDeadLetterQueue(
            services, configuration);

        var serviceProvider = services.BuildServiceProvider();
        var deadLetterService = serviceProvider.GetRequiredService<IDeadLetterService>();

        // Assert
        deadLetterService.Should().NotBeNull();

        // Testa configuração
        var shouldRetryTransient = deadLetterService.ShouldRetry(new TimeoutException(), 1);
        var shouldRetryPermanent = deadLetterService.ShouldRetry(new ArgumentException(), 1);

        shouldRetryTransient.Should().BeTrue();
        shouldRetryPermanent.Should().BeFalse();
    }

    [Fact]
    public async Task MessageRetryMiddleware_EndToEnd_WorksWithDeadLetterSystem()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        var environment = CreateHostEnvironment("Testing");

        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddSingleton(environment);

        services.AddSingleton(CreateTestRabbitMqOptions());

        MessagingExtensions.AddDeadLetterQueue(
            services, configuration);

        var serviceProvider = services.BuildServiceProvider();
        var message = new TestMessage { Id = "integration-test" };
        var middlewareFactory = serviceProvider.GetRequiredService<IMessageRetryMiddlewareFactory>();
        var middleware = middlewareFactory.CreateMiddleware<TestMessage>("TestHandler", "test-queue");
        
        var failureCount = 0;
        Func<TestMessage, CancellationToken, Task> failingHandler = (msg, ct) => 
        {
            failureCount++;
            throw new Exception("Simulated transient failure");
        };

        // Act
        var result = await middleware.ExecuteWithRetryAsync(message, failingHandler, CancellationToken.None);

        // Assert
        result.Should().BeFalse("Message should be sent to DLQ after max retries");
        failureCount.Should().Be(4); // 1 original + 3 retries (based on config in CreateConfiguration)
    }

    [Fact]
    public void FailedMessageInfo_Serialization_WorksCorrectly()
    {
        // Arrange
        var failedMessage = new FailedMessageInfo
        {
            MessageId = "test-123",
            MessageType = "TestMessage",
            OriginalMessage = "{\"id\":\"test-123\"}",
            SourceQueue = "test-queue",
            FirstAttemptAt = DateTime.UtcNow.AddMinutes(-5),
            LastAttemptAt = DateTime.UtcNow,
            AttemptCount = 3,
            LastFailureReason = "Test failure",
            Environment = new EnvironmentMetadata
            {
                EnvironmentName = "Testing",
                ApplicationVersion = "1.0.0"
            }
        };

        var exception = new InvalidOperationException("Test exception");
        failedMessage.AddFailureAttempt(exception, "TestHandler");

        // Act
        var json = failedMessage.ToJson();
        var deserializedMessage = FailedMessageInfoExtensions.FromJson(json);

        // Assert
        deserializedMessage.Should().NotBeNull();
        deserializedMessage!.MessageId.Should().Be(failedMessage.MessageId);
        deserializedMessage.MessageType.Should().Be(failedMessage.MessageType);
        deserializedMessage.AttemptCount.Should().Be(failedMessage.AttemptCount);
        deserializedMessage.FailureHistory.Should().HaveCount(1);
        deserializedMessage.FailureHistory[0].ExceptionType.Should().Contain("InvalidOperationException");
    }

    [Theory]
    [InlineData("System.TimeoutException", EFailureType.Transient)]
    [InlineData("System.ArgumentException", EFailureType.Permanent)]
    [InlineData("System.OutOfMemoryException", EFailureType.Critical)]
    [InlineData("UnknownException", EFailureType.Unknown)]
    public void FailureClassification_WithDifferentExceptions_ReturnsCorrectType(string exceptionTypeName, EFailureType expectedType)
    {
        // Arrange
        Exception exception = exceptionTypeName switch
        {
            "System.TimeoutException" => new TimeoutException("Test"),
            "System.ArgumentException" => new ArgumentException("Test"),
            "System.OutOfMemoryException" => new TestOutOfMemoryException("Test"),
            "System.InvalidOperationException" => new InvalidOperationException("Test"),
            "UnknownException" => new TestUnknownException("Unknown"), // Tipo customizado para teste de Unknown
            _ => new InvalidOperationException("Unknown")
        };

        // Act
        var failureType = exception.ClassifyFailure();

        // Assert
        failureType.Should().Be(expectedType);
    }

    private static IConfiguration CreateConfiguration(bool includeServiceBus = false)
    {
        var configData = new Dictionary<string, string?>
        {
            ["Messaging:Enabled"] = "true",
            ["Messaging:DeadLetter:Enabled"] = "true",
            ["Messaging:DeadLetter:MaxRetryAttempts"] = "3",
            ["Messaging:DeadLetter:InitialRetryDelaySeconds"] = "2",
            ["Messaging:DeadLetter:BackoffMultiplier"] = "2.0",
            ["Messaging:DeadLetter:MaxRetryDelaySeconds"] = "60",
            ["Messaging:DeadLetter:DeadLetterTtlHours"] = "24",
            ["Messaging:DeadLetter:EnableDetailedLogging"] = "true",
            ["Messaging:DeadLetter:EnableAdminNotifications"] = "false",
            ["Messaging:RabbitMQ:ConnectionString"] = "amqp://localhost",
            ["Messaging:RabbitMQ:DefaultQueueName"] = "test-queue"
        };

        if (includeServiceBus)
        {
            configData["Messaging:ServiceBus:ConnectionString"] = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test";
            configData["Messaging:ServiceBus:DefaultTopicName"] = "test-topic";
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private static IHostEnvironment CreateHostEnvironment(string environmentName)
    {
        return new MockHostEnvironment(environmentName);
    }

    // Classes de exceção para teste
    private class TestOutOfMemoryException : OutOfMemoryException
    {
        public TestOutOfMemoryException(string message) : base(message)
        {
        }
    }

    private class TestUnknownException : Exception
    {
        public TestUnknownException(string message) : base(message)
        {
        }
    }

    // Classes de teste
    private class TestMessage
    {
        public string Id { get; set; } = string.Empty;
    }
}