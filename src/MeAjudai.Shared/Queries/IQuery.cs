namespace MeAjudai.Shared.Queries;

public interface IQuery<TResult>
{
    Guid CorrelationId { get; }
}