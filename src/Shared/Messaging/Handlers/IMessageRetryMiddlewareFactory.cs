namespace MeAjudaAi.Shared.Messaging.Handlers;

/// <summary>
/// Factory para criar MessageRetryMiddleware
/// </summary>
public interface IMessageRetryMiddlewareFactory
{
    /// <summary>
    /// Cria middleware de retry para um tipo específico de mensagem
    /// </summary>
    MessageRetryMiddleware<TMessage> CreateMiddleware<TMessage>(string handlerType, string sourceQueue)
        where TMessage : class;
}
