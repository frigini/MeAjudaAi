using MeAjudaAi.Shared.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Queries;

/// <summary>
/// Default implementation of <see cref="IQueryDispatcher"/> that resolves query handlers
/// from the DI container and executes any registered pipeline behaviors.
/// </summary>
/// <remarks>
/// This dispatcher follows the mediator pattern, decoupling query senders from handlers.
/// It automatically applies all registered <see cref="IPipelineBehavior{TRequest,TResponse}"/>
/// instances (e.g., validation, logging, caching) around the handler execution.
/// </remarks>
public class QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger) : IQueryDispatcher
{
    /// <summary>
    /// Dispatches the specified query to its registered handler, executing all configured
    /// <see cref="IPipelineBehavior{TRequest,TResponse}"/> instances around it.
    /// </summary>
    /// <typeparam name="TQuery">The query type that implements <see cref="IQuery{TResult}"/>.</typeparam>
    /// <typeparam name="TResult">The query result type.</typeparam>
    /// <param name="query">The query instance to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The result produced by the query handler.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler is registered for the specified query type.
    /// </exception>
    public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        logger.LogInformation("Executing query {QueryType} with correlation {CorrelationId}",
            typeof(TQuery).Name, query.CorrelationId);

        return await ExecuteWithPipeline(query, async () =>
        {
            var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return await handler.HandleAsync(query, cancellationToken);
        }, cancellationToken);
    }

    private async Task<TResponse> ExecuteWithPipeline<TRequest, TResponse>(
        TRequest request,
        RequestHandlerDelegate<TResponse> handlerDelegate,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse();

        RequestHandlerDelegate<TResponse> pipeline = handlerDelegate;

        foreach (var behavior in behaviors)
        {
            var currentPipeline = pipeline;
            pipeline = () => behavior.Handle(request, currentPipeline, cancellationToken);
        }

        return await pipeline();
    }
}
