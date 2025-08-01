using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Queries;

public class QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger) : IQueryDispatcher
{
    public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        logger.LogInformation("Executing query {QueryType} with correlation {CorrelationId}",
            typeof(TQuery).Name, query.CorrelationId);

        return await handler.HandleAsync(query, cancellationToken);
    }
}