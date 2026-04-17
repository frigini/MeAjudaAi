using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Implementação mock de IMessageBus para testes E2E.
/// Não processa eventos para evitar deadlocks - os testes devem usar as APIs diretamente.
/// </summary>
internal class MockMessageBus : IMessageBus
{
    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        // No-op: testes E2E devem disparar ações via APIs HTTP, não via eventos
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task>? handler = null, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        // No-op: assinaturas não são realmente criadas em testes E2E
        return Task.CompletedTask;
    }
}

/// <summary>
/// Implementação mock de IDomainEventProcessor usada em testes E2E;
/// não processa eventos de domínio para evitar publicação de eventos de integração.
/// </summary>
internal class MockDomainEventProcessor : IDomainEventProcessor
{
    public Task ProcessDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        // No-op: eventos de domínio não são processados em testes E2E para evitar publicação de eventos de integração
        return Task.CompletedTask;
    }
}
