using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging;

/// <summary>
/// Implementação no-operation do IMessageBus para quando messaging está desabilitado
/// </summary>
internal class NoOpMessageBus : IMessageBus
{
    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        // Não faz nada quando messaging está desabilitado
        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        // Não faz nada quando messaging está desabilitado
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        // Não faz nada quando messaging está desabilitado
        return Task.CompletedTask;
    }
}