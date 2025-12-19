using System.Linq.Expressions;

namespace MeAjudaAi.Shared.Jobs;

/// <summary>
/// Implementação Null Object de IBackgroundJobService para cenários de teste/desabilitado.
/// Retorna sucesso sem realmente enfileirar jobs.
/// </summary>
public sealed class NoOpBackgroundJobService : IBackgroundJobService
{
    public Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan? delay = null) where T : notnull
    {
        return Task.CompletedTask;
    }

    public Task EnqueueAsync(Expression<Func<Task>> methodCall, TimeSpan? delay = null)
    {
        return Task.CompletedTask;
    }

    public Task ScheduleRecurringAsync(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        return Task.CompletedTask;
    }
}
