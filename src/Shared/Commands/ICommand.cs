using MeAjudaAi.Shared.Functional;
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
