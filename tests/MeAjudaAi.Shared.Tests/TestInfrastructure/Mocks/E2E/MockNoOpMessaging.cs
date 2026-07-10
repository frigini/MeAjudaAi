using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.E2E;

/// <summary>
/// Implementação no-op de IMessageBus para testes E2E que não precisam de processamento de mensagens.
/// </summary>
public class MockNoOpMessageBus : IMessageBus
{
    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task>? handler = null, string? subscriptionName = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

/// <summary>
/// Implementação no-op de IDomainEventProcessor para testes E2E que não precisam de processamento de eventos de domínio.
/// </summary>
public class MockNoOpDomainEventProcessor : IDomainEventProcessor
{
    public Task ProcessDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
