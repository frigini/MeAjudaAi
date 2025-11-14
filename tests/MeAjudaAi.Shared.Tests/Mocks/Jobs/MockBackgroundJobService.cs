using System.Linq.Expressions;
using MeAjudaAi.Shared.Jobs;

namespace MeAjudaAi.Shared.Tests.Mocks.Jobs;

/// <summary>
/// Mock do serviço de background jobs para uso em testes.
/// Apenas registra os jobs enfileirados sem executá-los.
/// </summary>
public class MockBackgroundJobService : IBackgroundJobService
{
    private readonly List<(Type JobType, object? State, TimeSpan? Delay)> _enqueuedJobs = [];

    public IReadOnlyList<(Type JobType, object? State, TimeSpan? Delay)> EnqueuedJobs => _enqueuedJobs.AsReadOnly();

    public string Enqueue<T>(object? state = null) where T : class
    {
        _enqueuedJobs.Add((typeof(T), state, null));
        return Guid.NewGuid().ToString();
    }

    public string Schedule<T>(TimeSpan delay, object? state = null) where T : class
    {
        _enqueuedJobs.Add((typeof(T), state, delay));
        return Guid.NewGuid().ToString();
    }

    public bool Delete(string jobId)
    {
        // Mock - sempre retorna true
        return true;
    }

    public void Clear()
    {
        _enqueuedJobs.Clear();
    }

    public Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan? delay = null) where T : notnull
    {
        throw new NotImplementedException();
    }

    public Task EnqueueAsync(Expression<Func<Task>> methodCall, TimeSpan? delay = null)
    {
        throw new NotImplementedException();
    }

    public Task ScheduleRecurringAsync(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        throw new NotImplementedException();
    }
}
