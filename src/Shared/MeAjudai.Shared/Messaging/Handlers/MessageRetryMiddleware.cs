using MeAjudaAi.Shared.Messaging.DeadLetter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.Handlers;

/// <summary>
/// Middleware para interceptar falhas em handlers de mensagens e implementar retry com Dead Letter Queue
/// </summary>
public sealed class MessageRetryMiddleware<TMessage>(
    IDeadLetterService deadLetterService,
    ILogger<MessageRetryMiddleware<TMessage>> logger,
    string handlerType,
    string sourceQueue) where TMessage : class
{

    /// <summary>
    /// Executa o handler com retry automático e Dead Letter Queue
    /// </summary>
    /// <param name="message">Mensagem a ser processada</param>
    /// <param name="handler">Handler que processará a mensagem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se processou com sucesso, False se enviou para DLQ</returns>
    public async Task<bool> ExecuteWithRetryAsync(
        TMessage message,
        Func<TMessage, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        var attemptCount = 0;
        Exception? lastException;

        while (true)
        {
            attemptCount++;

            try
            {
                logger.LogDebug("Processing message of type {MessageType}, attempt {AttemptCount}",
                    typeof(TMessage).Name, attemptCount);

                await handler(message, cancellationToken);

                if (attemptCount > 1)
                {
                    logger.LogInformation("Message of type {MessageType} processed successfully on attempt {AttemptCount}",
                        typeof(TMessage).Name, attemptCount);
                }

                return true; // Sucesso
            }
            catch (OperationCanceledException)
            {
                // Cancelamento deve ser propagado imediatamente, não tratado como falha
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;

                logger.LogWarning(ex,
                    "Failed to process message of type {MessageType} on attempt {AttemptCount}: {ErrorMessage}",
                    typeof(TMessage).Name, attemptCount, ex.Message);

                // Verifica se deve tentar novamente
                if (!deadLetterService.ShouldRetry(ex, attemptCount))
                {
                    logger.LogError(ex,
                        "Message of type {MessageType} failed permanently after {AttemptCount} attempts. Sending to dead letter queue.",
                        typeof(TMessage).Name, attemptCount);

                    await deadLetterService.SendToDeadLetterAsync(
                        message, ex, handlerType, sourceQueue, attemptCount, cancellationToken);

                    return false; // Enviado para DLQ
                }

                // Calcula delay para próxima tentativa
                var retryDelay = deadLetterService.CalculateRetryDelay(attemptCount);
                
                logger.LogInformation(
                    "Will retry message of type {MessageType} in {RetryDelay}ms (attempt {AttemptCount})",
                    typeof(TMessage).Name, retryDelay.TotalMilliseconds, attemptCount);

                await Task.Delay(retryDelay, cancellationToken);
            }
        }
    }
}

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
