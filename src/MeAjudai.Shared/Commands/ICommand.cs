namespace MeAjudaAi.Shared.Commands;

public interface ICommand
{
    Guid CorrelationId { get; }
}

public interface ICommand<TResult> : ICommand
{
}