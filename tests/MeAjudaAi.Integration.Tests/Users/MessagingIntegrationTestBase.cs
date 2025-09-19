using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Tests.Mocks.Messaging;

namespace MeAjudaAi.Integration.Tests.Users;

/// <summary>
/// Classe base para testes de integração que precisam verificar mensagens
/// </summary>
public abstract class MessagingIntegrationTestBase : Base.ApiTestBase
{
    protected MessagingMockManager MessagingMocks { get; private set; } = null!;

    public Task InitializeTestAsync()
    {
        // Obtém o gerenciador de mocks de messaging
        MessagingMocks = Factory.Services.GetRequiredService<MessagingMockManager>();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Limpa mensagens antes de cada teste
    /// </summary>
    protected async Task CleanMessagesAsync()
    {
        await CleanDatabaseAsync();
        
        // Inicializa o messaging se ainda não foi inicializado
        if (MessagingMocks == null)
        {
            await InitializeTestAsync();
        }
        
        MessagingMocks?.ClearAllMessages();
    }

    /// <summary>
    /// Verifica se uma mensagem específica foi publicada
    /// </summary>
    protected bool WasMessagePublished<T>(Func<T, bool>? predicate = null) where T : class
    {
        return MessagingMocks.WasMessagePublishedAnywhere(predicate);
    }

    /// <summary>
    /// Obtém todas as mensagens de um tipo específico
    /// </summary>
    protected IEnumerable<T> GetPublishedMessages<T>() where T : class
    {
        return MessagingMocks.GetAllPublishedMessages<T>();
    }

    /// <summary>
    /// Obtém estatísticas de mensagens publicadas
    /// </summary>
    protected MessagingStatistics GetMessagingStatistics()
    {
        return MessagingMocks.GetStatistics();
    }
}