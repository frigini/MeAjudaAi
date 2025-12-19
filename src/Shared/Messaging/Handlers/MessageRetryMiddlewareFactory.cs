using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Messaging.DeadLetter;

namespace MeAjudaAi.Shared.Messaging.Handlers;

/// <summary>
/// Implementação do factory para MessageRetryMiddleware
/// </summary>
public sealed class MessageRetryMiddlewareFactory(IServiceProvider serviceProvider) : IMessageRetryMiddlewareFactory
{
    public MessageRetryMiddleware<TMessage> CreateMiddleware<TMessage>(string handlerType, string sourceQueue)
        where TMessage : class
    {
        var deadLetterService = serviceProvider.GetRequiredService<IDeadLetterService>();
        var logger = serviceProvider.GetRequiredService<ILogger<MessageRetryMiddleware<TMessage>>>();

        return new MessageRetryMiddleware<TMessage>(deadLetterService, logger, handlerType, sourceQueue);
    }
}
