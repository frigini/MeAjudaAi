using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Messaging;
using Azure.Messaging.ServiceBus;
using System.Reflection;

namespace MeAjudaAi.Shared.Tests.Mocks.Messaging;

/// <summary>
/// Gerenciador para coordenar todos os mocks de messaging durante os testes
/// </summary>
public class MessagingMockManager(
    MockServiceBusMessageBus serviceBusMock,
    MockRabbitMqMessageBus rabbitMqMock,
    ILogger<MessagingMockManager> logger)
{

    /// <summary>
    /// Mock do Azure Service Bus
    /// </summary>
    public MockServiceBusMessageBus ServiceBus => serviceBusMock;

    /// <summary>
    /// Mock do RabbitMQ
    /// </summary>
    public MockRabbitMqMessageBus RabbitMq => rabbitMqMock;

    /// <summary>
    /// Limpa todas as mensagens publicadas em todos os mocks
    /// </summary>
    public void ClearAllMessages()
    {
        logger.LogInformation("Clearing all published messages from messaging mocks");
        
        serviceBusMock.ClearPublishedMessages();
        rabbitMqMock.ClearPublishedMessages();
    }

    /// <summary>
    /// Reinicia todos os mocks para o comportamento normal
    /// </summary>
    public void ResetAllMocks()
    {
        logger.LogInformation("Resetting all messaging mocks to normal behavior");
        
        serviceBusMock.ResetToNormalBehavior();
        rabbitMqMock.ResetToNormalBehavior();
        
        ClearAllMessages();
    }

    /// <summary>
    /// Obtém estatísticas de todas as mensagens publicadas
    /// </summary>
    public MessagingStatistics GetStatistics()
    {
        return new MessagingStatistics
        {
            ServiceBusMessageCount = serviceBusMock.PublishedMessages.Count,
            RabbitMqMessageCount = rabbitMqMock.PublishedMessages.Count,
            TotalMessageCount = serviceBusMock.PublishedMessages.Count + rabbitMqMock.PublishedMessages.Count
        };
    }

    /// <summary>
    /// Verifica se uma mensagem foi publicada em qualquer um dos sistemas de messaging
    /// </summary>
    public bool WasMessagePublishedAnywhere<T>(Func<T, bool>? predicate = null) where T : class
    {
        return serviceBusMock.WasMessagePublished(predicate) || 
               rabbitMqMock.WasMessagePublished(predicate);
    }

    /// <summary>
    /// Obtém todas as mensagens de um tipo que foram publicadas em qualquer sistema
    /// </summary>
    public IEnumerable<T> GetAllPublishedMessages<T>() where T : class
    {
        var serviceBusMessages = serviceBusMock.GetPublishedMessages<T>();
        var rabbitMqMessages = rabbitMqMock.GetPublishedMessages<T>();
        
        return serviceBusMessages.Concat(rabbitMqMessages);
    }
}

/// <summary>
/// Estatísticas de mensagens publicadas
/// </summary>
public class MessagingStatistics
{
    public int ServiceBusMessageCount { get; set; }
    public int RabbitMqMessageCount { get; set; }
    public int TotalMessageCount { get; set; }
}

/// <summary>
/// Extensions para configurar os mocks de messaging nos testes
/// </summary>
public static class MessagingMockExtensions
{
    /// <summary>
    /// Adiciona os mocks de messaging ao container de DI usando Scrutor onde aplicável
    /// </summary>
    public static IServiceCollection AddMessagingMocks(this IServiceCollection services)
    {
        // Remove implementações reais se existirem
        RemoveRealImplementations(services);
        
        // Usa Scrutor para registrar automaticamente todos os mocks de messaging do assembly atual
        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes
                .Where(type => type.Namespace != null && 
                              type.Namespace.Contains("Messaging") &&
                              type.Name.StartsWith("Mock")))
            .AsSelf()
            .WithSingletonLifetime());
        
        // Registra específicos que precisam de configuração especial
        services.AddSingleton<MessagingMockManager>();
        
        // Registra os mocks como as implementações do IMessageBus
        services.AddSingleton<IMessageBus>(provider => provider.GetRequiredService<MockServiceBusMessageBus>());
        
        return services;
    }

    /// <summary>
    /// Remove implementações reais dos sistemas de messaging
    /// </summary>
    private static void RemoveRealImplementations(IServiceCollection services)
    {
        // Remove ServiceBusClient se registrado
        var serviceBusDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ServiceBusClient));
        if (serviceBusDescriptor != null)
        {
            services.Remove(serviceBusDescriptor);
        }

        // Remove outras implementações de IMessageBus
        var messageBusDescriptors = services.Where(d => d.ServiceType == typeof(IMessageBus)).ToList();
        foreach (var descriptor in messageBusDescriptors)
        {
            services.Remove(descriptor);
        }
    }
}