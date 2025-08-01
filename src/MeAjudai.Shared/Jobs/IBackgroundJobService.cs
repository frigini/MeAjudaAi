using System.Linq.Expressions;

namespace MeAjudaAi.Shared.Jobs;

public interface IBackgroundJobService
{
    Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan? delay = null);

    Task EnqueueAsync(Expression<Func<Task>> methodCall, TimeSpan? delay = null);

    Task ScheduleRecurringAsync(string jobId, Expression<Func<Task>> methodCall, string cronExpression);
}