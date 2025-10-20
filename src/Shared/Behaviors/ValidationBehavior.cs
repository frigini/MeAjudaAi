using FluentValidation;
using MeAjudaAi.Shared.Mediator;

namespace MeAjudaAi.Shared.Behaviors;

/// <summary>
/// Behavior para validação automática de requests usando FluentValidation.
/// Intercepta todos os Commands e Queries e executa as validações correspondentes antes do handler.
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição (Command/Query)</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
/// <remarks>
/// Inicializa uma nova instância do ValidationBehavior.
/// </remarks>
/// <param name="validators">Coleção de validadores para o tipo de request</param>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{

    /// <summary>
    /// Executa a validação antes de chamar o próximo handler na pipeline.
    /// </summary>
    /// <param name="request">A requisição sendo processada</param>
    /// <param name="next">Delegate para o próximo handler na pipeline</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>A resposta do handler se a validação for bem-sucedida</returns>
    /// <exception cref="MeAjudaAi.Shared.Exceptions.ValidationException">Lançada quando há erros de validação</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new Exceptions.ValidationException(failures);
        }

        return await next();
    }
}
