using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.E2E;

/// <summary>
/// No-op implementation of IMessageBus for E2E tests that don't need message processing.
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
/// No-op implementation of IDomainEventProcessor for E2E tests that don't need event processing.
/// </summary>
public class MockNoOpDomainEventProcessor : IDomainEventProcessor
{
    public Task ProcessDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
