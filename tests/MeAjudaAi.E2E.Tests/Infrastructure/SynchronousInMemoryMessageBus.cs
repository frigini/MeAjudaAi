using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Uma implementação simples de MessageBus em memória que executa os handlers sincronicamente.
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

        // Resolve todos os IEventHandler registrados para este tipo de mensagem via Reflection
        // para contornar a falta de restrições na interface IMessageBus
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

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        // Não implementado para esta versão simplificada de teste
        return Task.CompletedTask;
    }
}
