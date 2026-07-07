using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Messaging;

/// <summary>
/// Implementação síncrona de IMessageBus em memória que executa handlers diretamente.
/// Útil para testes E2E onde queremos validar consistência eventual de forma determinística.
/// </summary>
public class SynchronousInMemoryMessageBus(
    IServiceProvider serviceProvider,
    ILogger<SynchronousInMemoryMessageBus> logger) : IMessageBus
{
    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        return PublishAsync(message, queueName, cancellationToken);
    }

    public async Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        if (@event == null) return;

        logger.LogInformation("SynchronousInMemoryMessageBus: Dispatching {EventType}", typeof(TMessage).Name);

        using var scope = serviceProvider.CreateScope();
        var handlerType = typeof(IEventHandler<>).MakeGenericType(typeof(TMessage));
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler == null) continue;

            var method = handlerType.GetMethod("HandleAsync");
            if (method != null)
            {
                await (Task)method.Invoke(handler, [@event, cancellationToken])!;
            }
        }
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task>? handler = null, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
