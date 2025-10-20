using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.Mocks.Messaging;

namespace MeAjudaAi.Integration.Tests.Users;

/// <summary>
/// Classe base para testes de integração que precisam verificar mensagens
/// </summary>
public abstract class MessagingIntegrationTestBase : Base.ApiTestBase
{
    protected MockServiceBusMessageBus ServiceBusMock { get; private set; } = null!;
    protected MockRabbitMqMessageBus RabbitMqMock { get; private set; } = null!;

    public Task InitializeTestAsync()
    {
        // Obtém os mocks individuais de messaging
        ServiceBusMock = Factory.Services.GetRequiredService<MockServiceBusMessageBus>();
        RabbitMqMock = Factory.Services.GetRequiredService<MockRabbitMqMessageBus>();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Limpa mensagens antes de cada teste
    /// </summary>
    protected async Task CleanMessagesAsync()
    {
        await ResetDatabaseAsync();

        // Inicializa o messaging se ainda não foi inicializado
        if (ServiceBusMock == null || RabbitMqMock == null)
        {
            await InitializeTestAsync();
        }

        // Limpa mensagens de todos os mocks
        ServiceBusMock?.ClearPublishedMessages();
        RabbitMqMock?.ClearPublishedMessages();
    }

    /// <summary>
    /// Verifica se uma mensagem específica foi publicada em qualquer sistema
    /// </summary>
    protected bool WasMessagePublished<T>(Func<T, bool>? predicate = null) where T : class
    {
        return ServiceBusMock.WasMessagePublished(predicate) ||
               RabbitMqMock.WasMessagePublished(predicate);
    }

    /// <summary>
    /// Obtém todas as mensagens de um tipo específico de todos os sistemas
    /// </summary>
    protected IEnumerable<T> GetPublishedMessages<T>() where T : class
    {
        var serviceBusMessages = ServiceBusMock.GetPublishedMessages<T>();
        var rabbitMqMessages = RabbitMqMock.GetPublishedMessages<T>();
        return serviceBusMessages.Concat(rabbitMqMessages);
    }

    /// <summary>
    /// Obtém estatísticas de mensagens publicadas
    /// </summary>
    protected MessagingStatistics GetMessagingStatistics()
    {
        return new MessagingStatistics
        {
            ServiceBusMessageCount = ServiceBusMock.PublishedMessages.Count,
            RabbitMqMessageCount = RabbitMqMock.PublishedMessages.Count,
            TotalMessageCount = ServiceBusMock.PublishedMessages.Count + RabbitMqMock.PublishedMessages.Count
        };
    }
}
