namespace MeAjudaAi.Shared.Commands;

public abstract record Command : ICommand
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
}

public abstract record Command<TResult> : ICommand<TResult>
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
}