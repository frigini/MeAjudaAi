using System.Linq.Expressions;
using MeAjudaAi.Shared.Jobs;

namespace MeAjudaAi.Integration.Tests.Mocks;

/// <summary>
/// Mock do IBackgroundJobService para testes de integração.
/// Não executa jobs em background, apenas registra chamadas.
/// </summary>
public class MockBackgroundJobService : IBackgroundJobService
{
    public List<string> EnqueuedJobs { get; } = new();

    public Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan? delay = null) where T : notnull
    {
        var methodName = (methodCall.Body as MethodCallExpression)?.Method.Name ?? "Unknown";
        var jobInfo = $"{typeof(T).Name}.{methodName}";
        EnqueuedJobs.Add(jobInfo);
        return Task.CompletedTask;
    }

    public Task EnqueueAsync(Expression<Func<Task>> methodCall, TimeSpan? delay = null)
    {
        var methodName = (methodCall.Body as MethodCallExpression)?.Method.Name ?? "Unknown";
        EnqueuedJobs.Add(methodName);
        return Task.CompletedTask;
    }

    public Task ScheduleRecurringAsync(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        EnqueuedJobs.Add($"Recurring:{jobId}");
        return Task.CompletedTask;
    }
}
