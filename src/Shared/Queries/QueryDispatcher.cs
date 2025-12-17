using MeAjudaAi.Shared.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Queries;

/// <summary>
/// Implementação padrão de <see cref="IQueryDispatcher"/> que resolve query handlers
/// do container de DI e executa quaisquer comportamentos de pipeline registrados.
/// </summary>
/// <remarks>
/// Este dispatcher segue o padrão mediator, desacoplando remetentes de queries dos handlers.
/// Ele aplica automaticamente todas as instâncias registradas de <see cref="IPipelineBehavior{TRequest,TResponse}"/>
/// (ex: validação, logging, caching) em torno da execução do handler.
/// <para>
/// <strong>Registro:</strong> Deve ser registrado como singleton no container de DI
/// via <c>services.AddSingleton&lt;IQueryDispatcher, QueryDispatcher&gt;()</c>.
/// </para>
/// </remarks>
public class QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger) : IQueryDispatcher
{
    /// <summary>
    /// Despacha a query especificada para seu handler registrado, executando todas as instâncias
    /// configuradas de <see cref="IPipelineBehavior{TRequest,TResponse}"/> ao redor dela.
    /// </summary>
    /// <typeparam name="TQuery">O tipo da query que implementa <see cref="IQuery{TResult}"/>.</typeparam>
    /// <typeparam name="TResult">O tipo do resultado da query.</typeparam>
    /// <param name="query">A instância da query a despachar.</param>
    /// <param name="cancellationToken">Token de cancelamento para a operação.</param>
    /// <returns>O resultado produzido pelo query handler.</returns>
    /// <exception cref="InvalidOperationException">
    /// Lançado quando nenhum handler está registrado para o tipo de query especificado.
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
