namespace MeAjudaAi.Shared.Messaging;

public interface IMessageBus
{
    Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default);

    Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default);

    Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default);
}
