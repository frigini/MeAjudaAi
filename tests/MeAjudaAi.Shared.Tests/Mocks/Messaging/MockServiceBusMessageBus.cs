using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Mocks.Messaging;

/// <summary>
/// Mock para Azure Service Bus para uso em testes
/// </summary>
public class MockServiceBusMessageBus : IMessageBus
{
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly ILogger<MockServiceBusMessageBus> _logger;
    private readonly List<(object message, string? destination, EMessageType type)> _publishedMessages = [];

    public MockServiceBusMessageBus(ILogger<MockServiceBusMessageBus> logger)
    {
        _mockMessageBus = new Mock<IMessageBus>();
        _logger = logger;

        SetupMockBehavior();
    }

    /// <summary>
    /// Lista de mensagens publicadas durante os testes
    /// </summary>
    public IReadOnlyList<(object message, string? destination, EMessageType type)> PublishedMessages
        => _publishedMessages.AsReadOnly();

    /// <summary>
    /// Limpa a lista de mensagens publicadas
    /// </summary>
    public void ClearPublishedMessages()
    {
        _publishedMessages.Clear();
    }

    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock Service Bus: Sending message of type {MessageType} to queue {QueueName}",
            typeof(TMessage).Name, queueName);

        _publishedMessages.Add((message!, queueName, EMessageType.Send));

        return _mockMessageBus.Object.SendAsync(message, queueName, cancellationToken);
    }

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock Service Bus: Publishing event of type {EventType} to topic {TopicName}",
            typeof(TMessage).Name, topicName);

        _publishedMessages.Add((@event!, topicName, EMessageType.Publish));

        return _mockMessageBus.Object.PublishAsync(@event, topicName, cancellationToken);
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock Service Bus: Subscribing to messages of type {MessageType} with subscription {SubscriptionName}",
            typeof(TMessage).Name, subscriptionName);

        return _mockMessageBus.Object.SubscribeAsync(handler, subscriptionName, cancellationToken);
    }

    private void SetupMockBehavior()
    {
        _mockMessageBus
            .Setup(x => x.SendAsync(It.IsAny<It.IsAnyType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMessageBus
            .Setup(x => x.PublishAsync(It.IsAny<It.IsAnyType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMessageBus
            .Setup(x => x.SubscribeAsync(It.IsAny<Func<It.IsAnyType, CancellationToken, Task>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Verifica se uma mensagem específica foi enviada
    /// </summary>
    public bool WasMessageSent<T>(Func<T, bool>? predicate = null) where T : class
    {
        var messagesOfType = _publishedMessages
            .Where(x => x.message is T && x.type == EMessageType.Send)
            .Select(x => (T)x.message);

        return predicate == null
            ? messagesOfType.Any()
            : messagesOfType.Any(predicate);
    }

    /// <summary>
    /// Verifica se um evento específico foi publicado
    /// </summary>
    public bool WasEventPublished<T>(Func<T, bool>? predicate = null) where T : class
    {
        var eventsOfType = _publishedMessages
            .Where(x => x.message is T && x.type == EMessageType.Publish)
            .Select(x => (T)x.message);

        return predicate == null
            ? eventsOfType.Any()
            : eventsOfType.Any(predicate);
    }

    /// <summary>
    /// Verifica se uma mensagem foi publicada (send ou publish)
    /// </summary>
    public bool WasMessagePublished<T>(Func<T, bool>? predicate = null) where T : class
    {
        return WasMessageSent(predicate) || WasEventPublished(predicate);
    }

    /// <summary>
    /// Obtém todas as mensagens de um tipo específico que foram enviadas
    /// </summary>
    public IEnumerable<T> GetSentMessages<T>() where T : class
    {
        return _publishedMessages
            .Where(x => x.message is T && x.type == EMessageType.Send)
            .Select(x => (T)x.message);
    }

    /// <summary>
    /// Obtém todos os eventos de um tipo específico que foram publicados
    /// </summary>
    public IEnumerable<T> GetPublishedEvents<T>() where T : class
    {
        return _publishedMessages
            .Where(x => x.message is T && x.type == EMessageType.Publish)
            .Select(x => (T)x.message);
    }

    /// <summary>
    /// Obtém todas as mensagens de um tipo específico (send + publish)
    /// </summary>
    public IEnumerable<T> GetPublishedMessages<T>() where T : class
    {
        return _publishedMessages
            .Where(x => x.message is T)
            .Select(x => (T)x.message);
    }

    /// <summary>
    /// Verifica se uma mensagem foi enviada para uma fila específica
    /// </summary>
    public bool WasMessageSentToQueue(string queueName)
    {
        return _publishedMessages.Any(x => x.destination == queueName && x.type == EMessageType.Send);
    }

    /// <summary>
    /// Verifica se um evento foi publicado para um tópico específico
    /// </summary>
    public bool WasEventPublishedToTopic(string topicName)
    {
        return _publishedMessages.Any(x => x.destination == topicName && x.type == EMessageType.Publish);
    }

    /// <summary>
    /// Simula uma falha no envio de mensagem
    /// </summary>
    public void SimulateSendFailure(Exception exception)
    {
        _mockMessageBus
            .Setup(x => x.SendAsync(It.IsAny<It.IsAnyType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    /// <summary>
    /// Simula uma falha na publicação de evento
    /// </summary>
    public void SimulatePublishFailure(Exception exception)
    {
        _mockMessageBus
            .Setup(x => x.PublishAsync(It.IsAny<It.IsAnyType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    /// <summary>
    /// Restaura o comportamento normal após simular uma falha
    /// </summary>
    public void ResetToNormalBehavior()
    {
        SetupMockBehavior();
    }
}

/// <summary>
/// Tipo de mensagem para tracking.
/// Nota: Usa prefixo 'E' para consistência com convenções do projeto,
/// embora a convenção moderna C# prefira 'MessageType'.
/// </summary>
public enum EMessageType
{
    None = 0,
    Send,
    Publish
}
