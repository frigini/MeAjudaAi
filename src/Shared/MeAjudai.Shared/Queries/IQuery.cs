namespace MeAjudaAi.Shared.Queries;

public interface IQuery<TResult>
{
    Guid CorrelationId { get; }
}