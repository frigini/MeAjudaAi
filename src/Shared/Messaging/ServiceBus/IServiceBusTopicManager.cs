namespace MeAjudaAi.Shared.Messaging.ServiceBus;

public interface IServiceBusTopicManager
{
    Task EnsureTopicsExistAsync(CancellationToken cancellationToken = default);

    Task CreateTopicIfNotExistsAsync(string topicName, CancellationToken cancellationToken = default);

    Task CreateSubscriptionIfNotExistsAsync(string topicName, string subscriptionName,
        string? filter = null, CancellationToken cancellationToken = default);
}