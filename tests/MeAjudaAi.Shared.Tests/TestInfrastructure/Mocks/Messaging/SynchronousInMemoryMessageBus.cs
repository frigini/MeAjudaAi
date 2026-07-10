using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Messaging;

/// <summary>
/// Implementação síncrona de IMessageBus em memória que executa handlers diretamente.
/// Útil para testes E2E onde queremos validar consistência eventual de forma determinística.
/// </summary>
public class SynchronousInMemoryMessageBus(
    IServiceProvider serviceProvider,
    ILogger<SynchronousInMemoryMessageBus> logger) : IMessageBus
{
    /// <summary>Cache estático de MethodInfo por tipo de mensagem para evitar overhead de reflexão a cada publish.</summary>
    private static readonly ConcurrentDictionary<Type, MethodInfo> _methodCache = new();

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

        var method = _methodCache.GetOrAdd(
            typeof(TMessage),
            _ => handlerType.GetMethod("HandleAsync")!);

        foreach (var handler in handlers)
        {
            if (handler == null) continue;

            try
            {
                await (Task)method.Invoke(handler, [@event, cancellationToken])!;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                // Rethrow the real handler failure, not the reflection wrapper
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task>? handler = null, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
