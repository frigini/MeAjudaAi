using System.Linq.Expressions;
using MeAjudaAi.Shared.Jobs;

namespace MeAjudaAi.Integration.Tests.Mocks;

/// <summary>
/// Mock do IBackgroundJobService para testes de integração.
/// Não executa jobs em background, apenas registra chamadas.
/// </summary>
public class MockBackgroundJobService : IBackgroundJobService
{
    private readonly object _lock = new();
    private readonly List<string> _enqueuedJobs = [];

    public IReadOnlyList<string> EnqueuedJobs
    {
        get { lock (_lock) { return _enqueuedJobs.ToList(); } }
    }

    public Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan? delay = null) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(methodCall);
        var methodName = (methodCall.Body as MethodCallExpression)?.Method.Name ?? "Unknown";
        var jobInfo = $"{typeof(T).Name}.{methodName}";
        lock (_lock) { _enqueuedJobs.Add(jobInfo); }
        return Task.CompletedTask;
    }

    public Task EnqueueAsync(Expression<Func<Task>> methodCall, TimeSpan? delay = null)
    {
        ArgumentNullException.ThrowIfNull(methodCall);
        var methodName = (methodCall.Body as MethodCallExpression)?.Method.Name ?? "Unknown";
        lock (_lock) { _enqueuedJobs.Add(methodName); }
        return Task.CompletedTask;
    }

    public Task ScheduleRecurringAsync(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        ArgumentNullException.ThrowIfNull(jobId);
        ArgumentNullException.ThrowIfNull(methodCall);
        lock (_lock) { _enqueuedJobs.Add($"Recurring:{jobId}"); }
        return Task.CompletedTask;
    }
}
