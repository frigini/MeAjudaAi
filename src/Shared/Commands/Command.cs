using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Shared.Commands;

public abstract record Command : ICommand
{
    public Guid CorrelationId { get; } = UuidGenerator.NewId();
}

public abstract record Command<TResult> : ICommand<TResult>
{
    public Guid CorrelationId { get; } = UuidGenerator.NewId();
}
