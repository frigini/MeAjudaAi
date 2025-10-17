namespace MeAjudaAi.Shared.Mediator;

/// <summary>
/// Interface base para todas as requisições no sistema CQRS.
/// Marcador interface para Commands e Queries.
/// </summary>
/// <typeparam name="TResponse">Tipo da resposta esperada</typeparam>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Interface para interceptação de pipeline em handlers CQRS.
/// Permite a implementação de aspectos transversais como validação, logging, cache, etc.
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição (Command/Query)</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Executa o comportamento do pipeline.
    /// </summary>
    /// <param name="request">A requisição sendo processada</param>
    /// <param name="next">Delegate para o próximo handler na pipeline</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>A resposta do handler</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>
/// Delegate que representa o próximo handler na pipeline.
/// </summary>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();