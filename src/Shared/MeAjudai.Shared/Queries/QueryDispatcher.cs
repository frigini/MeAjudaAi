using MeAjudaAi.Shared.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Queries;

public class QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger) : IQueryDispatcher
{
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