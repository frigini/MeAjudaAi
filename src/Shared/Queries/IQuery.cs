using MeAjudaAi.Shared.Mediator;

namespace MeAjudaAi.Shared.Queries;

public interface IQuery<out TResult> : IRequest<TResult>
{
    Guid CorrelationId { get; }
}
