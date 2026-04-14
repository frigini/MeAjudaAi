using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Mock implementation of IMessageBus for E2E tests.
/// Does not process events to avoid deadlocks - tests should use APIs directly.
/// </summary>
internal class MockMessageBus : IMessageBus
{
    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        // No-op: E2E tests should trigger actions via HTTP APIs, not via events
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        // No-op: subscriptions are not actually created in E2E tests
        return Task.CompletedTask;
    }
}

/// <summary>
/// Mock implementation of IDomainEventProcessor for E2E tests
/// Does not process domain events to avoid integration event publication
/// </summary>
internal class MockDomainEventProcessor : IDomainEventProcessor
{
    public Task ProcessDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        // No-op: domain events are not processed in E2E tests to avoid integration event publication
        return Task.CompletedTask;
    }
}
