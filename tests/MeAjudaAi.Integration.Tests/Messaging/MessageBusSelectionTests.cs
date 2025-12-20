using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Factories;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using MeAjudaAi.Shared.Messaging.Strategy;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Messaging;

/// <summary>
/// Testes para verificar se o MessageBus correto é selecionado baseado no ambiente
/// </summary>
[Collection("Integration Tests Collection")]
public class MessageBusSelectionTests : ApiTestBase
{
    [Fact]
    public void MessageBusFactory_InTestingEnvironment_ShouldReturnMock()
    {
        // Arrange & Act
        var messageBus = Services.GetRequiredService<IMessageBus>();

        // Assert
        // Em ambiente de Testing, devemos ter o mock configurado pelos testes
        messageBus.Should().NotBeNull("MessageBus deve estar configurado");

        // Verifica se não é uma implementação real (ServiceBus ou RabbitMQ)
        messageBus.Should().NotBeOfType<ServiceBusMessageBus>("Não deve usar ServiceBus em testes");
        messageBus.Should().NotBeOfType<RabbitMqMessageBus>("Não deve usar RabbitMQ real em testes");
    }

    [Fact]
    public void MessageBusFactory_InDevelopmentEnvironment_ShouldCreateRabbitMq()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Registrar IConfiguration no DI
        services.AddSingleton<IConfiguration>(configuration);

        // Simular ambiente Development
        services.AddSingleton<IHostEnvironment>(new MockHostEnvironment("Development"));
        services.AddSingleton<ILogger<MessageBusFactory>>(new TestLogger<MessageBusFactory>());
        services.AddSingleton<ILogger<RabbitMqMessageBus>>(new TestLogger<RabbitMqMessageBus>());
        services.AddSingleton<ILogger<ServiceBusMessageBus>>(new TestLogger<ServiceBusMessageBus>());

        // Configurar opções mínimas
        services.AddSingleton(new RabbitMqOptions { ConnectionString = "amqp://localhost", DefaultQueueName = "test" });
        services.AddSingleton(new ServiceBusOptions { ConnectionString = "Endpoint=sb://test/", DefaultTopicName = "test" });
        services.AddSingleton(new MessageBusOptions());

        // Registrar implementações
        services.AddSingleton<RabbitMqMessageBus>();
        services.AddSingleton<ServiceBusMessageBus>();
        services.AddSingleton<IMessageBusFactory, MessageBusFactory>();

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IMessageBusFactory>();

        // Act
        var messageBus = factory.CreateMessageBus();

        // Assert
        messageBus.Should().BeOfType<RabbitMqMessageBus>("Development deve usar RabbitMQ");
    }

    [Fact]
    public void MessageBusFactory_InProductionEnvironment_ShouldCreateServiceBus()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Registrar IConfiguration no DI
        services.AddSingleton<IConfiguration>(configuration);

        // Simular ambiente Production
        services.AddSingleton<IHostEnvironment>(new MockHostEnvironment("Production"));
        services.AddSingleton<ILogger<MessageBusFactory>>(new TestLogger<MessageBusFactory>());
        services.AddSingleton<ILogger<RabbitMqMessageBus>>(new TestLogger<RabbitMqMessageBus>());
        services.AddSingleton<ILogger<ServiceBusMessageBus>>(new TestLogger<ServiceBusMessageBus>());

        // Configurar opções mínimas
        var serviceBusOptions = new ServiceBusOptions
        {
            ConnectionString = "Endpoint=sb://test/;SharedAccessKeyName=test;SharedAccessKey=test",
            DefaultTopicName = "test"
        };

        services.AddSingleton(new RabbitMqOptions { ConnectionString = "amqp://localhost", DefaultQueueName = "test" });
        services.AddSingleton(serviceBusOptions);
        services.AddSingleton(new MessageBusOptions());

        // Registrar ServiceBusClient para ServiceBusMessageBus
        services.AddSingleton(serviceProvider => new Azure.Messaging.ServiceBus.ServiceBusClient(serviceBusOptions.ConnectionString));

        // Registrar dependências necessárias
        services.AddSingleton<IEventTypeRegistry, EventTypeRegistry>();
        services.AddSingleton<ITopicStrategySelector, TopicStrategySelector>();

        // Registrar implementações
        services.AddSingleton<RabbitMqMessageBus>();
        services.AddSingleton<ServiceBusMessageBus>();
        services.AddSingleton<IMessageBusFactory, MessageBusFactory>();

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IMessageBusFactory>();

        // Act
        var messageBus = factory.CreateMessageBus();

        // Assert
        messageBus.Should().BeOfType<ServiceBusMessageBus>("Production deve usar Azure Service Bus");
    }
}

/// <summary>
/// Logger de teste que não faz nada
/// </summary>
public class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
