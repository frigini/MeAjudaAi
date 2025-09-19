using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Messaging;
using Azure.Messaging.ServiceBus;

namespace MeAjudaAi.Shared.Tests.Mocks.Messaging;

/// <summary>
/// Gerenciador para coordenar todos os mocks de messaging durante os testes
/// </summary>
public class MessagingMockManager
{
    private readonly MockServiceBusMessageBus _serviceBusMock;
    private readonly MockRabbitMqMessageBus _rabbitMqMock;
    private readonly ILogger<MessagingMockManager> _logger;

    public MessagingMockManager(
        MockServiceBusMessageBus serviceBusMock,
        MockRabbitMqMessageBus rabbitMqMock,
        ILogger<MessagingMockManager> logger)
    {
        _serviceBusMock = serviceBusMock;
        _rabbitMqMock = rabbitMqMock;
        _logger = logger;
    }

    /// <summary>
    /// Mock do Azure Service Bus
    /// </summary>
    public MockServiceBusMessageBus ServiceBus => _serviceBusMock;

    /// <summary>
    /// Mock do RabbitMQ
    /// </summary>
    public MockRabbitMqMessageBus RabbitMq => _rabbitMqMock;

    /// <summary>
    /// Limpa todas as mensagens publicadas em todos os mocks
    /// </summary>
    public void ClearAllMessages()
    {
        _logger.LogInformation("Clearing all published messages from messaging mocks");
        
        _serviceBusMock.ClearPublishedMessages();
        _rabbitMqMock.ClearPublishedMessages();
    }

    /// <summary>
    /// Reinicia todos os mocks para o comportamento normal
    /// </summary>
    public void ResetAllMocks()
    {
        _logger.LogInformation("Resetting all messaging mocks to normal behavior");
        
        _serviceBusMock.ResetToNormalBehavior();
        _rabbitMqMock.ResetToNormalBehavior();
        
        ClearAllMessages();
    }

    /// <summary>
    /// Obtém estatísticas de todas as mensagens publicadas
    /// </summary>
    public MessagingStatistics GetStatistics()
    {
        return new MessagingStatistics
        {
            ServiceBusMessageCount = _serviceBusMock.PublishedMessages.Count,
            RabbitMqMessageCount = _rabbitMqMock.PublishedMessages.Count,
            TotalMessageCount = _serviceBusMock.PublishedMessages.Count + _rabbitMqMock.PublishedMessages.Count
        };
    }

    /// <summary>
    /// Verifica se uma mensagem foi publicada em qualquer um dos sistemas de messaging
    /// </summary>
    public bool WasMessagePublishedAnywhere<T>(Func<T, bool>? predicate = null) where T : class
    {
        return _serviceBusMock.WasMessagePublished(predicate) || 
               _rabbitMqMock.WasMessagePublished(predicate);
    }

    /// <summary>
    /// Obtém todas as mensagens de um tipo que foram publicadas em qualquer sistema
    /// </summary>
    public IEnumerable<T> GetAllPublishedMessages<T>() where T : class
    {
        var serviceBusMessages = _serviceBusMock.GetPublishedMessages<T>();
        var rabbitMqMessages = _rabbitMqMock.GetPublishedMessages<T>();
        
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
    /// Adiciona os mocks de messaging ao container de DI
    /// </summary>
    public static IServiceCollection AddMessagingMocks(this IServiceCollection services)
    {
        // Remove implementações reais se existirem
        RemoveRealImplementations(services);
        
        // Adiciona os mocks
        services.AddSingleton<MockServiceBusMessageBus>();
        services.AddSingleton<MockRabbitMqMessageBus>();
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