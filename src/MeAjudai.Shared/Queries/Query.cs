namespace MeAjudaAi.Shared.Queries;

public abstract record Query<TResult> : IQuery<TResult>
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
}