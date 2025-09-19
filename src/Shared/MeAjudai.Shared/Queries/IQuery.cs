using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Shared.Queries;

public interface IQuery<TResult> : IRequest<TResult>
{
    Guid CorrelationId { get; }
}