using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Mediator;

namespace MeAjudaAi.Shared.Commands;

public interface ICommand : IRequest<Unit>
{
    Guid CorrelationId { get; }
}

public interface ICommand<out TResult> : IRequest<TResult>
{
    Guid CorrelationId { get; }
}
