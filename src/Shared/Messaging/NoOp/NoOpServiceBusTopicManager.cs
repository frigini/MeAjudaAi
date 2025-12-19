using MeAjudaAi.Shared.Messaging.ServiceBus;

namespace MeAjudaAi.Shared.Messaging.NoOp;

/// <summary>
/// Implementação no-operation do IServiceBusTopicManager para quando messaging está desabilitado
/// </summary>
internal class NoOpServiceBusTopicManager : IServiceBusTopicManager
{
    public Task EnsureTopicsExistAsync(CancellationToken cancellationToken = default)
    {
        // Não faz nada quando messaging está desabilitado
        return Task.CompletedTask;
    }

    public Task CreateTopicIfNotExistsAsync(string topicName, CancellationToken cancellationToken = default)
    {
        // Não faz nada quando messaging está desabilitado
        return Task.CompletedTask;
    }

    public Task CreateSubscriptionIfNotExistsAsync(string topicName, string subscriptionName, string? filter = null, CancellationToken cancellationToken = default)
    {
        // Não faz nada quando messaging está desabilitado
        return Task.CompletedTask;
    }
}
