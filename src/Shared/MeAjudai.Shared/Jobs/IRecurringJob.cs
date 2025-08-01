namespace MeAjudaAi.Shared.Jobs;

public interface IRecurringJob
{
    string JobId { get; }

    string CronExpression { get; }

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}