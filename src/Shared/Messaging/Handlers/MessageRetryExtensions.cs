using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Messaging.Handlers;

/// <summary>
/// Extensões para facilitar o uso do middleware de retry
/// </summary>
public static class MessageRetryExtensions
{
    /// <summary>
    /// Executa um handler de mensagem com retry automático e Dead Letter Queue
    /// </summary>
    /// <typeparam name="TMessage">Tipo da mensagem</typeparam>
    /// <param name="message">Mensagem a ser processada</param>
    /// <param name="handler">Handler que processará a mensagem</param>
    /// <param name="serviceProvider">Service provider para resolver dependências</param>
    /// <param name="sourceQueue">Fila de origem da mensagem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se processou com sucesso, False se enviou para DLQ</returns>
    public static async Task<bool> ExecuteWithRetryAsync<TMessage>(
        this TMessage message,
        Func<TMessage, CancellationToken, Task> handler,
        IServiceProvider serviceProvider,
        string sourceQueue,
        CancellationToken cancellationToken = default) where TMessage : class
    {
        var middlewareFactory = serviceProvider.GetRequiredService<IMessageRetryMiddlewareFactory>();
        var handlerType = handler.Method.DeclaringType?.FullName ?? "Unknown";

        var middleware = middlewareFactory.CreateMiddleware<TMessage>(handlerType, sourceQueue);

        return await middleware.ExecuteWithRetryAsync(message, handler, cancellationToken);
    }

    /// <summary>
    /// Configura o middleware de retry para handlers de eventos
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection para chaining</returns>
    public static IServiceCollection AddMessageRetryMiddleware(this IServiceCollection services)
    {
        services.AddScoped<IMessageRetryMiddlewareFactory, MessageRetryMiddlewareFactory>();
        return services;
    }
}
